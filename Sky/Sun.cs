using System.Numerics;

namespace Sky;

/// <summary>
/// The light of the sun or the moon where it reaches a place, its illuminance on a surface facing
/// it, its color and that color's temperature.
/// </summary>
/// <param name="Illuminance">Lux on a surface facing the light, 0 where it is below the
/// horizon.</param>
/// <param name="Color">The light's color in linear sRGB at a luminance of 1, so the illuminance
/// carries all of its strength.</param>
/// <param name="Temperature">The color's correlated color temperature in kelvin, 0 where the light
/// is out.</param>
public readonly record struct CelestialLight(float Illuminance, Vector3 Color, float Temperature);

/// <summary>The sun's light outside the air and where it reaches the ground through it.</summary>
/// <remarks>
/// Outside the air the sun gives the same light at every height over the horizon, 1,361 watts a
/// square meter at about 94 lumens a watt, 128,000 lux, in the color of a black body at the sun's
/// effective temperature. Through the air the molecules take more of its blue than of its red and
/// the way through grows long as the sun sinks, so the light that reaches the ground dims and warms
/// toward the horizon.
/// </remarks>
public static class Sun
{
    /// <summary>The sun's illuminance outside the air in lux on a surface facing it.</summary>
    public const float OutsideIlluminance = 128_000;

    /// <summary>
    /// The sun's effective temperature in kelvin, the black body's whose color its light outside
    /// the air is taken to have.
    /// </summary>
    public const float Temperature = 5_772;

    /// <summary>The sun's light outside the air in linear sRGB, its luminance in lux.</summary>
    public static Vector3 Outside { get; } = Colorimetry.FromTemperature(Temperature) * OutsideIlluminance;

    /// <summary>
    /// The sun's light at <paramref name="altitude"/> meters through <paramref name="air"/>, with
    /// the sun <paramref name="elevation"/> degrees over the horizon there.
    /// </summary>
    /// <remarks>
    /// The sun is taken as a point, so it is out from the moment its middle meets the ground's
    /// horizon, and the light scattered toward the place by the sky above is the sky's
    /// (<see cref="SkyLight"/>), none of it counted here.
    /// </remarks>
    public static CelestialLight At(Atmosphere air, float elevation, float altitude = 0) => Through(air, Outside, elevation, altitude);

    /// <summary>
    /// A light of <paramref name="outside"/> in linear sRGB outside the air, through it from
    /// <paramref name="elevation"/> degrees.
    /// </summary>
    internal static CelestialLight Through(Atmosphere air, Vector3 outside, float elevation, float altitude)
    {
        var through = outside * air.Transmittance(altitude, MathF.Sin(float.DegreesToRadians(elevation)));
        var lux = Colorimetry.Luminance(through);
        return lux <= 0 ? new CelestialLight(0, Vector3.Zero, 0) : new CelestialLight(lux, through / lux, Colorimetry.TemperatureOf(through));
    }
}
