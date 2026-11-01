using System.Numerics;

namespace Sky;

/// <summary>
/// The light the sky gives, the sun's light scattered by the air toward a place, as luminance along
/// each way from it and as illuminance on a level surface there, for the sun outside the air of
/// <see cref="Sun.Outside"/>.
/// </summary>
/// <remarks>
/// <para>
/// Along a way from the eye the air scatters the sun's light toward the eye once, by the
/// molecules' phase function and Cornette and Shanks's for the aerosols, each part dimmed by the
/// air between the sun and the point and between the point and the eye. The light scattered more
/// than once is Hillaire's approximation of 2020, which takes the light at a point scattered twice
/// or more as alike from every way, the second order's luminance divided by one less the share the
/// air sends on at each order. A way that meets the ground takes the ground's light, the sun's
/// light it returns by its albedo, and the second order carries the ground's light too.
/// </para>
/// <para>
/// The light's strength scales with the sun's outside the air, so an engine whose sun is given
/// another illuminance multiplies what this returns by its sun's over <see
/// cref="Sun.OutsideIlluminance"/>.
/// The two tables the light is read from, the transmittance by height and zenith and the light
/// scattered more than once by height and the sun's zenith, are made when the sky is.
/// </para>
/// </remarks>
public sealed class SkyLight
{
    // The tables' sizes, heights along one axis and the cosine of the zenith along the other. The
    // transmittance's heights crowd near the ground as the square of their count, where the sky is
    // seen from, and its angles are fine enough that the horizon's edge spans half a degree.
    private const int TransmittanceHeights = 32, TransmittanceAngles = 256, ScatterHeights = 32, ScatterAngles = 32;

    // The steps along a way, from the eye and over the 64 ways the light scattered more than once
    // is gathered from.
    private const int ViewSteps = 48, ScatterSteps = 20, ScatterWays = 8;

    private readonly Vector3[] _transmittance = new Vector3[TransmittanceHeights * TransmittanceAngles];
    private readonly Vector3[] _scattered = new Vector3[ScatterHeights * ScatterAngles];

    /// <summary>
    /// Makes the sky of <paramref name="air"/>, its tables made at once over the processor's cores,
    /// a row of heights to a core, the light scattered more than once read from the
    /// transmittance's.
    /// </summary>
    public SkyLight(Atmosphere air)
    {
        Air = air;
        Parallel.For(0, TransmittanceHeights, h =>
        {
            for (int a = 0; a < TransmittanceAngles; a++)
                _transmittance[h * TransmittanceAngles + a] = air.Transmittance(TransmittanceHeight(h), -1 + 2f * a / (TransmittanceAngles - 1));
        });
        Parallel.For(0, ScatterHeights, h =>
        {
            for (int a = 0; a < ScatterAngles; a++)
                _scattered[h * ScatterAngles + a] = Scattered(air.Top * h / (ScatterHeights - 1), -1 + 2f * a / (ScatterAngles - 1));
        });
    }

    /// <summary>The air the sky is made of.</summary>
    public Atmosphere Air { get; }

    /// <summary>
    /// The sky's luminance in candelas a square meter along <paramref name="view"/> from
    /// <paramref name="altitude"/> meters, with the sun toward <paramref name="sunToward"/>, both
    /// with +Y up, in linear sRGB whose luminance is the sky's.
    /// </summary>
    /// <remarks>The sun's own disc is left out, which an engine draws by its angular size from <see
    /// cref="Sun.At"/>.</remarks>
    public Vector3 Luminance(Vector3 view, Vector3 sunToward, float altitude = 0)
    {
        view = Vector3.Normalize(view);
        sunToward = Vector3.Normalize(sunToward);
        var (toGround, toTop) = Air.Distances(altitude, view.Y);
        var distance = toGround ?? toTop;
        var cosAngle = Vector3.Dot(view, sunToward);
        var (rayleighPhase, aerosolPhase) = (RayleighPhase(cosAngle), AerosolPhase(Air.AerosolAnisotropy, cosAngle));
        var (light, through) = (Vector3.Zero, Vector3.One);
        for (int i = 0; i < ViewSteps; i++)
        {
            var (from, to) = ((float)i / ViewSteps, (float)(i + 1) / ViewSteps);
            var (near, far) = (from * from * distance, to * to * distance);
            var (height, up) = At(altitude, view, (near + far) / 2);
            var sunZenith = Vector3.Dot(up, sunToward);
            var (rayleigh, aerosol) = (Air.RayleighAt(height), Air.AerosolAt(height) * Air.AerosolAlbedo);
            var source = (rayleigh * rayleighPhase + aerosol * aerosolPhase) * TransmittanceAt(height, sunZenith)
                + (rayleigh + aerosol) * ScatteredAt(height, sunZenith);
            (light, through) = Step(light, through, source, Air.ExtinctionAt(height), far - near);
        }
        if (toGround is { } ground)
        {
            var (_, up) = At(altitude, view, ground);
            var sunZenith = Vector3.Dot(up, sunToward);
            light += through * TransmittanceAt(0, sunZenith) * MathF.Max(sunZenith, 0) * Air.GroundAlbedo / MathF.PI;
        }
        return light * Sun.Outside;
    }

    /// <summary>
    /// The illuminance in lux the sky gives a level surface at <paramref name="altitude"/> meters,
    /// with the sun toward <paramref name="sunToward"/>, in linear sRGB whose luminance is the lux,
    /// the sun's own light apart.
    /// </summary>
    public Vector3 Illuminance(Vector3 sunToward, float altitude = 0)
    {
        // The dome in bands of the cosine with the zenith, each the same solid angle, and around.
        const int Bands = 24, Around = 48;
        var sum = Vector3.Zero;
        for (int b = 0; b < Bands; b++)
        {
            var cos = (b + 0.5f) / Bands;
            var sin = MathF.Sqrt(1 - cos * cos);
            for (int a = 0; a < Around; a++)
            {
                var turn = 2 * MathF.PI * (a + 0.5f) / Around;
                sum += Luminance(new Vector3(sin * MathF.Cos(turn), cos, sin * MathF.Sin(turn)), sunToward, altitude) * cos;
            }
        }
        return sum * (2 * MathF.PI / (Bands * Around));
    }

    /// <summary>
    /// The share of the light that crosses the air from <paramref name="altitude"/> toward a way of
    /// cosine <paramref name="cosZenith"/> with the zenith, from the table.
    /// </summary>
    internal Vector3 TransmittanceAt(float altitude, float cosZenith)
    {
        var row = MathF.Sqrt(Math.Clamp(altitude / Air.Top, 0, 1)) * (TransmittanceHeights - 1);
        var column = (Math.Clamp(cosZenith, -1, 1) + 1) / 2 * (TransmittanceAngles - 1);
        return Bilinear(_transmittance, TransmittanceAngles, row, column);
    }

    /// <summary>
    /// The light scattered twice or more at <paramref name="altitude"/> with the sun at a cosine of
    /// <paramref name="cosSunZenith"/> with the zenith, for a sun of 1 outside the air, from the
    /// table.
    /// </summary>
    internal Vector3 ScatteredAt(float altitude, float cosSunZenith)
    {
        var row = Math.Clamp(altitude / Air.Top, 0, 1) * (ScatterHeights - 1);
        var column = (Math.Clamp(cosSunZenith, -1, 1) + 1) / 2 * (ScatterAngles - 1);
        return Bilinear(_scattered, ScatterAngles, row, column);
    }

    // The transmittance table's height at a row, crowded near the ground.
    private float TransmittanceHeight(int row)
    {
        var along = (float)row / (TransmittanceHeights - 1);
        return Air.Top * along * along;
    }

    // Hillaire's light scattered twice or more at a height with the sun at a cosine with the
    // zenith,
    // gathered over ways spread evenly over the sphere, each marched in even steps.
    private Vector3 Scattered(float altitude, float cosSunZenith)
    {
        var sunToward = new Vector3(MathF.Sqrt(MathF.Max(0, 1 - cosSunZenith * cosSunZenith)), cosSunZenith, 0);
        const float Isotropic = 1 / (4 * MathF.PI);
        var (second, sent) = (Vector3.Zero, Vector3.Zero);
        for (int i = 0; i < ScatterWays; i++)
            for (int j = 0; j < ScatterWays; j++)
            {
                var cos = 1 - 2 * (i + 0.5f) / ScatterWays;
                var sin = MathF.Sqrt(1 - cos * cos);
                var turn = 2 * MathF.PI * (j + 0.5f) / ScatterWays;
                var way = new Vector3(sin * MathF.Cos(turn), cos, sin * MathF.Sin(turn));
                var (toGround, toTop) = Air.Distances(altitude, cos);
                var step = (toGround ?? toTop) / ScatterSteps;
                var (light, onward, through) = (Vector3.Zero, Vector3.Zero, Vector3.One);
                for (int s = 0; s < ScatterSteps; s++)
                {
                    var (height, up) = At(altitude, way, (s + 0.5f) * step);
                    var scattering = Air.RayleighAt(height) + Air.AerosolAt(height) * Air.AerosolAlbedo;
                    var extinction = Air.ExtinctionAt(height);
                    var span = Span(extinction, step);
                    light += through * scattering * TransmittanceAt(height, Vector3.Dot(up, sunToward)) * Isotropic * span;
                    onward += through * scattering * span;
                    through *= Exp(-extinction * step);
                }
                if (toGround is { } ground)
                {
                    var (_, up) = At(altitude, way, ground);
                    var sunZenith = Vector3.Dot(up, sunToward);
                    light += through * TransmittanceAt(0, sunZenith) * MathF.Max(sunZenith, 0) * Air.GroundAlbedo / MathF.PI;
                }
                // Each way's share of the sphere times the phase function alike everywhere, 1 / N.
                second += light / (ScatterWays * ScatterWays);
                sent += onward / (ScatterWays * ScatterWays);
            }
        return second / (Vector3.One - sent);
    }

    // The height and the zenith at a distance along a way from a height, the ground's curve turning
    // the zenith under it.
    private (float Height, Vector3 Up) At(float altitude, Vector3 way, float distance)
    {
        var point = new Vector3(0, Air.PlanetRadius + altitude, 0) + way * distance;
        var radius = point.Length();
        return (radius - Air.PlanetRadius, point / radius);
    }

    // A step of a march, the light the step's source adds over its length dimmed by the air within
    // it, as Hillaire integrates a step.
    private static (Vector3 Light, Vector3 Through) Step(Vector3 light, Vector3 through, Vector3 source, Vector3 extinction, float length) =>
        (light + through * source * Span(extinction, length), through * Exp(-extinction * length));

    // The integral of the transmittance over a step, (1 - e^(-σ l)) / σ, or the length where the
    // air there takes nearly nothing.
    private static Vector3 Span(Vector3 extinction, float length)
    {
        static float One(float sigma, float length) => sigma * length < 1e-5f ? length : (1 - MathF.Exp(-sigma * length)) / sigma;
        return new Vector3(One(extinction.X, length), One(extinction.Y, length), One(extinction.Z, length));
    }

    private static Vector3 Exp(Vector3 v) => new(MathF.Exp(v.X), MathF.Exp(v.Y), MathF.Exp(v.Z));

    // The molecules' phase function, 3 / (16 pi) (1 + cos^2).
    private static float RayleighPhase(float cos) => 3 / (16 * MathF.PI) * (1 + cos * cos);

    // Cornette and Shanks's phase function for the aerosols, Henyey and Greenstein's form with the
    // molecules' 1 + cos^2 in it, normalized again.
    private static float AerosolPhase(float g, float cos) =>
        3 / (8 * MathF.PI) * (1 - g * g) * (1 + cos * cos) / ((2 + g * g) * MathF.Pow(MathF.Max(1 + g * g - 2 * g * cos, 1e-6f), 1.5f));

    private static Vector3 Bilinear(Vector3[] table, int width, float row, float column)
    {
        var (r0, c0) = ((int)row, (int)column);
        var (r1, c1) = (Math.Min(r0 + 1, table.Length / width - 1), Math.Min(c0 + 1, width - 1));
        var (fr, fc) = (row - r0, column - c0);
        var top = Vector3.Lerp(table[r0 * width + c0], table[r0 * width + c1], fc);
        var bottom = Vector3.Lerp(table[r1 * width + c0], table[r1 * width + c1], fc);
        return Vector3.Lerp(top, bottom, fr);
    }
}
