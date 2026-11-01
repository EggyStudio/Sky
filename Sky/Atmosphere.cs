using System.Numerics;

namespace Sky;

/// <summary>
/// The air around a planet as a sky is drawn and lit from it, the planet's radius, the molecules'
/// scattering, the aerosols' extinction and the ozone's absorption with the heights each thins
/// over, and the ground's albedo, in meters and per meter at sea level.
/// </summary>
/// <remarks>
/// <para>
/// The molecules and the aerosols thin with height as an exponential of their scale heights, and
/// the ozone lies in a layer from <see cref="OzoneBottom"/> up by <see cref="OzoneWidth"/>, densest
/// in its middle and thinning to nothing at either edge. A coefficient is given at three
/// wavelengths, 680, 550 and 440 nanometers, read as linear sRGB's red, green and blue, so a color
/// a coefficient multiplies is a color of the frame.
/// </para>
/// <para>
/// <see cref="Earth"/> holds the Earth of Unity's HDRP, whose physically based sky the values are
/// taken from so an engine's sky can be read beside one drawn there. Its ozone's absorption is
/// HDRP's times the 0.6 HDRP scales the layer's optical depth by, so the layer here absorbs as much
/// as HDRP's does.
/// </para>
/// </remarks>
public sealed record Atmosphere
{
    /// <summary>The Earth's air on a clear day, with HDRP's values.</summary>
    public static Atmosphere Earth { get; } = new();

    /// <summary>The planet's radius in meters, 6,378,100 for the Earth at the equator.</summary>
    public float PlanetRadius { get; init; } = 6_378_100;

    /// <summary>
    /// How much of the light the molecules scatter a meter at sea level, at 680, 550 and 440
    /// nanometers, which they take nothing of otherwise.
    /// </summary>
    public Vector3 RayleighScattering { get; init; } = new(5.8e-6f, 13.5e-6f, 33.1e-6f);

    /// <summary>
    /// The height in meters over which the molecules thin to a share of 1/e, 8,000 for the Earth.
    /// </summary>
    public float RayleighScaleHeight { get; init; } = 8_000;

    /// <summary>
    /// How much of the light the aerosols take out a meter at sea level, scattered or absorbed,
    /// alike at every wavelength.
    /// </summary>
    /// <remarks>
    /// <see cref="Turbidity"/> reads the air's haze from it and <see cref="WithTurbidity"/> sets it
    /// from one.
    /// </remarks>
    public float AerosolExtinction { get; init; } = 10e-6f;

    /// <summary>
    /// The share of the light the aerosols take out that they scatter, the rest absorbed, at each
    /// wavelength.
    /// </summary>
    public Vector3 AerosolAlbedo { get; init; } = new(0.9f);

    /// <summary>
    /// The height in meters over which the aerosols thin to a share of 1/e, 1,200 for the Earth.
    /// </summary>
    public float AerosolScaleHeight { get; init; } = 1_200;

    /// <summary>
    /// How strongly the aerosols scatter forward, from -1 for all of the light back the way it came
    /// to 1 for all of it on, as Cornette and Shanks's phase function takes it.
    /// </summary>
    public float AerosolAnisotropy { get; init; } = 0.8f;

    /// <summary>
    /// How much of the light the ozone absorbs a meter in the middle of its layer, at each
    /// wavelength.
    /// </summary>
    public Vector3 OzoneAbsorption { get; init; } = new Vector3(0.65e-6f, 1.88e-6f, 0.08e-6f) * 0.6f;

    /// <summary>The height in meters the ozone's layer starts at.</summary>
    public float OzoneBottom { get; init; } = 20_000;

    /// <summary>The ozone layer's depth in meters, densest halfway up it.</summary>
    public float OzoneWidth { get; init; } = 20_000;

    /// <summary>
    /// The share of the light the ground returns at each wavelength, scattered evenly.
    /// </summary>
    public Vector3 GroundAlbedo { get; init; } = new(0.12f, 0.10f, 0.09f);

    /// <summary>
    /// The height in meters past which the air is taken to end, where its thickest layer has
    /// thinned to a thousandth or the ozone's layer ends, whichever is higher.
    /// </summary>
    public float Top => MathF.Max(-MathF.Log(0.001f) * MathF.Max(RayleighScaleHeight, AerosolScaleHeight), OzoneBottom + OzoneWidth);

    /// <summary>
    /// The air's haze, the optical depth of the molecules and the aerosols over that of the
    /// molecules alone straight up at 550 nanometers, 1 for air with no aerosols in it and about
    /// 1.1 for the Earth here.
    /// </summary>
    public float Turbidity => 1 + AerosolExtinction * AerosolScaleHeight / (RayleighScattering.Y * RayleighScaleHeight);

    /// <summary>
    /// This air with the aerosols' extinction set so its <see cref="Turbidity"/> is <paramref
    /// name="turbidity"/>, 1 at least.
    /// </summary>
    public Atmosphere WithTurbidity(float turbidity) =>
        this with { AerosolExtinction = MathF.Max(turbidity - 1, 0) * RayleighScattering.Y * RayleighScaleHeight / AerosolScaleHeight };

    /// <summary>
    /// How much of the light the molecules scatter a meter at <paramref name="altitude"/> meters,
    /// at each wavelength.
    /// </summary>
    public Vector3 RayleighAt(float altitude) => RayleighScattering * MathF.Exp(-MathF.Max(altitude, 0) / RayleighScaleHeight);

    /// <summary>
    /// How much of the light the aerosols take out a meter at <paramref name="altitude"/> meters.
    /// </summary>
    public float AerosolAt(float altitude) => AerosolExtinction * MathF.Exp(-MathF.Max(altitude, 0) / AerosolScaleHeight);

    /// <summary>
    /// How much of the light the ozone absorbs a meter at <paramref name="altitude"/> meters, at
    /// each wavelength.
    /// </summary>
    public Vector3 OzoneAt(float altitude) => OzoneAbsorption * MathF.Max(0, 1 - MathF.Abs(2 * (altitude - OzoneBottom) / OzoneWidth - 1));

    /// <summary>
    /// How much of the light the air takes out a meter at <paramref name="altitude"/> meters,
    /// scattered or absorbed, at each wavelength.
    /// </summary>
    public Vector3 ExtinctionAt(float altitude) => RayleighAt(altitude) + new Vector3(AerosolAt(altitude)) + OzoneAt(altitude);

    /// <summary>
    /// The share of the light that crosses the air from <paramref name="altitude"/> meters toward a
    /// way whose cosine with the zenith there is <paramref name="cosZenith"/>, out to the top of
    /// the
    /// air, at each wavelength, or nothing where the way meets the ground.
    /// </summary>
    /// <remarks>
    /// The planet is round, so a way below the level of the eye still leaves the air where it
    /// passes over the ground's horizon. The optical depth is summed over steps that are short near
    /// the start, where the air is thickest along a way that starts low, and lengthen toward the
    /// top.
    /// </remarks>
    public Vector3 Transmittance(float altitude, float cosZenith)
    {
        var (toGround, toTop) = Distances(altitude, cosZenith);
        if (toGround is not null) return Vector3.Zero;
        var depth = OpticalDepth(altitude, cosZenith, toTop, 64);
        return new Vector3(MathF.Exp(-depth.X), MathF.Exp(-depth.Y), MathF.Exp(-depth.Z));
    }

    /// <summary>
    /// The optical depth of the air over <paramref name="distance"/> meters from
    /// <paramref name="altitude"/> toward a way whose cosine with the zenith is
    /// <paramref name="cosZenith"/>, summed over <paramref name="steps"/> steps.
    /// </summary>
    internal Vector3 OpticalDepth(float altitude, float cosZenith, float distance, int steps)
    {
        var depth = Vector3.Zero;
        for (int i = 0; i < steps; i++)
        {
            // The steps lengthen as the square of their count, from the start out.
            var (from, to) = ((float)i / steps, (float)(i + 1) / steps);
            var (near, far) = (from * from * distance, to * to * distance);
            depth += ExtinctionAt(AltitudeAlong(altitude, cosZenith, (near + far) / 2)) * (far - near);
        }
        return depth;
    }

    /// <summary>
    /// The altitude in meters <paramref name="distance"/> meters from <paramref name="altitude"/>
    /// toward a way whose cosine with the zenith is <paramref name="cosZenith"/>.
    /// </summary>
    internal float AltitudeAlong(float altitude, float cosZenith, float distance)
    {
        // In doubles, since the radius squared is some 4e13, past which a float keeps half a meter.
        double r = PlanetRadius + (double)altitude, t = distance;
        return (float)(Math.Sqrt(r * r + t * t + 2 * r * t * cosZenith) - PlanetRadius);
    }

    /// <summary>
    /// The cosine with the zenith at <paramref name="distance"/> meters along the way, which turns
    /// as the ground curves under it.
    /// </summary>
    internal float CosZenithAlong(float altitude, float cosZenith, float distance)
    {
        double r = PlanetRadius + (double)altitude, t = distance;
        var at = Math.Sqrt(r * r + t * t + 2 * r * t * cosZenith);
        return (float)((r * cosZenith + t) / at);
    }

    /// <summary>
    /// How far a way from <paramref name="altitude"/> whose cosine with the zenith is
    /// <paramref name="cosZenith"/> runs to the ground, or null where it misses it, and to the top
    /// of the air, 0 where it starts above it and leaves.
    /// </summary>
    internal (float? Ground, float Top) Distances(float altitude, float cosZenith)
    {
        double r = PlanetRadius + (double)MathF.Max(altitude, 0), mu = cosZenith, top = PlanetRadius + (double)Top;
        var outward = r * r * (mu * mu - 1) + top * top;
        var toTop = outward > 0 ? Math.Max(0, -r * mu + Math.Sqrt(outward)) : 0;
        var inward = r * r * (mu * mu - 1) + (double)PlanetRadius * PlanetRadius;
        float? toGround = mu < 0 && inward >= 0 ? (float)Math.Max(0, -r * mu - Math.Sqrt(inward)) : null;
        return (toGround, (float)toTop);
    }
}
