using System.Numerics;

namespace Sky;

/// <summary>
/// Colors in linear sRGB, its primaries and D65's white, and the color temperatures of light, to
/// and from a black body's.
/// </summary>
public static class Colorimetry
{
    /// <summary>
    /// The luminance of a color in linear sRGB, by BT.709's weights, so a color of luminance 1
    /// carries a hue alone.
    /// </summary>
    public static float Luminance(Vector3 rgb) => 0.2126f * rgb.X + 0.7152f * rgb.Y + 0.0722f * rgb.Z;

    /// <summary>
    /// The color of a black body at <paramref name="kelvin"/>, from 1,667 to 25,000, in linear sRGB
    /// at a luminance of 1.
    /// </summary>
    /// <remarks>
    /// The chromaticity is read off the Planckian locus by Kim and others' cubic fit in CIE 1931's
    /// xy, and a channel a red light's would take below 0 is held at 0.
    /// </remarks>
    public static Vector3 FromTemperature(float kelvin)
    {
        double t = Math.Clamp(kelvin, 1_667f, 25_000f);
        var x = t <= 4_000
            ? -0.2661239e9 / (t * t * t) - 0.2343589e6 / (t * t) + 0.8776956e3 / t + 0.179910
            : -3.0258469e9 / (t * t * t) + 2.1070379e6 / (t * t) + 0.2226347e3 / t + 0.240390;
        var y = t <= 2_222 ? -1.1063814 * x * x * x - 1.34811020 * x * x + 2.18555832 * x - 0.20219683
            : t <= 4_000 ? -0.9549476 * x * x * x - 1.37418593 * x * x + 2.09137015 * x - 0.16748867
            : 3.0817580 * x * x * x - 5.87338670 * x * x + 3.75112997 * x - 0.37001483;
        var rgb = Vector3.Max(FromXyz(new Vector3((float)(x / y), 1, (float)((1 - x - y) / y))), Vector3.Zero);
        return rgb / Luminance(rgb);
    }

    /// <summary>
    /// The correlated color temperature of a color in linear sRGB, the black body's whose color is
    /// nearest it, by McCamy's cubic in CIE 1931's xy.
    /// </summary>
    /// <remarks>
    /// A black body's color from <see cref="FromTemperature"/> reads back within 25 K from 2,000 to
    /// 6,500 K and 130 K under at 10,000 (<c>ColorimetryTests</c>), the cubic drifting from the
    /// locus toward the blue.
    /// </remarks>
    public static float TemperatureOf(Vector3 rgb)
    {
        var xyz = ToXyz(rgb);
        var sum = xyz.X + xyz.Y + xyz.Z;
        if (sum <= 0) return 0;
        var (x, y) = (xyz.X / sum, xyz.Y / sum);
        var n = (x - 0.3320f) / (0.1858f - y);
        return 449 * n * n * n + 3525 * n * n + 6823.3f * n + 5520.33f;
    }

    // Linear sRGB to and from CIE 1931's XYZ, D65 white.
    private static Vector3 ToXyz(Vector3 c) => new(
        0.4124564f * c.X + 0.3575761f * c.Y + 0.1804375f * c.Z,
        0.2126729f * c.X + 0.7151522f * c.Y + 0.0721750f * c.Z,
        0.0193339f * c.X + 0.1191920f * c.Y + 0.9503041f * c.Z);

    private static Vector3 FromXyz(Vector3 c) => new(
        3.2404542f * c.X - 1.5371385f * c.Y - 0.4985314f * c.Z,
        -0.9692660f * c.X + 1.8760108f * c.Y + 0.0415560f * c.Z,
        0.0556434f * c.X - 0.2040259f * c.Y + 1.0572252f * c.Z);
}
