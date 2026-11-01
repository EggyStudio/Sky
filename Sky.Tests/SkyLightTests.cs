using System.Numerics;
using Xunit;

namespace Sky.Tests;

public class SkyLightTests
{
    private static readonly SkyLight Sky = new(Atmosphere.Earth);

    private static Vector3 SunAt(float elevation) =>
        new(MathF.Cos(float.DegreesToRadians(elevation)), MathF.Sin(float.DegreesToRadians(elevation)), 0);

    [Fact]
    public void The_Clear_Sky_Is_Blue_Overhead_And_Brighter_Toward_The_Horizon()
    {
        var sun = SunAt(54);
        var zenith = Sky.Luminance(Vector3.UnitY, sun);
        Assert.True(zenith.Z > zenith.Y && zenith.Y > zenith.X, $"zenith {zenith}");
        Assert.InRange(Colorimetry.Luminance(zenith), 1_000, 3_000);
        var horizon = Sky.Luminance(new Vector3(-1, 0.02f, 0), sun);
        Assert.True(Colorimetry.Luminance(horizon) > 2 * Colorimetry.Luminance(zenith), $"horizon {horizon}, zenith {zenith}");
    }

    [Fact]
    public void The_Sky_Lights_A_Level_Surface_With_A_Tenth_Of_What_The_Sun_Does()
    {
        // 7,860 lux at 54 degrees against the sun's 88,900 on the level, the air of HDRP's Earth
        // holding little haze.
        var sun = Sun.At(Atmosphere.Earth, 54);
        var level = sun.Illuminance * MathF.Sin(float.DegreesToRadians(54));
        var sky = Colorimetry.Luminance(Sky.Illuminance(SunAt(54)));
        Assert.InRange(level, 88_000, 89_800);
        Assert.InRange(sky, 7_700, 8_020);
    }

    [Fact]
    public void The_Sky_Darkens_Past_Sunset()
    {
        var day = Colorimetry.Luminance(Sky.Luminance(Vector3.UnitY, SunAt(54)));
        var dusk = Colorimetry.Luminance(Sky.Luminance(Vector3.UnitY, SunAt(-3)));
        var night = Colorimetry.Luminance(Sky.Luminance(Vector3.UnitY, SunAt(-18)));
        Assert.InRange(dusk / day, 0.005f, 0.05f);
        Assert.True(night < 0.01f * dusk, $"night {night}, dusk {dusk}");
    }

    [Fact]
    public void Haze_Brightens_The_Sky_As_It_Dims_The_Sun()
    {
        var hazy = new SkyLight(Atmosphere.Earth.WithTurbidity(3));
        var clear = Colorimetry.Luminance(Sky.Illuminance(SunAt(30)));
        Assert.True(Colorimetry.Luminance(hazy.Illuminance(SunAt(30))) > clear);
    }
}
