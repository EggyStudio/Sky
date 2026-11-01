using Xunit;

namespace Sky.Tests;

public class SunTests
{
    [Fact]
    public void The_Sun_At_54_Degrees_Reaches_The_Ground_At_110000_Lux_And_5000_Kelvin()
    {
        // The reference scene, 27 June at 15:05 in Cologne, sets its sun 53.97 degrees high to
        // 97,050 lux at 5,633 K outside the air, which HDRP's Earth dims to 73,900 lux at 4,924 K
        // at
        // the ground, the sun's color the same within a hundred kelvin and its lux three quarters
        // of this, since that sun outside the air is 120,000 lux times the sine of its height.
        var sun = Sun.At(Atmosphere.Earth, 53.97f);
        Assert.InRange(sun.Illuminance, 108_900, 111_100);
        Assert.InRange(sun.Temperature, 4_976, 5_076);
        Assert.Equal(1, Colorimetry.Luminance(sun.Color), 0.001f);
    }

    [Fact]
    public void The_Sun_Dims_And_Warms_As_It_Sinks_And_Is_Out_Below_The_Horizon()
    {
        var (lux, kelvin) = (float.MaxValue, float.MaxValue);
        foreach (var elevation in new[] { 90f, 60, 30, 15, 5, 1 })
        {
            var sun = Sun.At(Atmosphere.Earth, elevation);
            Assert.True(sun.Illuminance < lux && sun.Temperature < kelvin, $"at {elevation} degrees, {sun.Illuminance} lux at {sun.Temperature} K");
            (lux, kelvin) = (sun.Illuminance, sun.Temperature);
        }
        Assert.Equal(0, Sun.At(Atmosphere.Earth, -2).Illuminance);
    }

    [Fact]
    public void Haze_Dims_The_Sun()
    {
        var clear = Sun.At(Atmosphere.Earth, 30).Illuminance;
        var hazy = Sun.At(Atmosphere.Earth.WithTurbidity(3), 30).Illuminance;
        Assert.InRange(hazy / clear, 0.5f, 0.8f);
    }
}
