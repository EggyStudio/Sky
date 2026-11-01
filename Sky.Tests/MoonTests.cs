using Xunit;

namespace Sky.Tests;

public class MoonTests
{
    [Fact]
    public void The_Full_Moon_Gives_A_Third_Of_A_Lux_And_The_Quarter_A_Tenth_Of_That()
    {
        var full = Moon.OutsideIlluminance(0);
        Assert.Equal(0.313f, full, 0.002f);
        Assert.InRange(Moon.OutsideIlluminance(90) / full, 0.07f, 0.12f);
        Assert.True(Moon.OutsideIlluminance(180) < 1e-3f);
    }

    [Fact]
    public void The_Moon_Dims_Through_The_Air_As_The_Sun_Does()
    {
        var moon = Moon.At(Atmosphere.Earth, 30, 0);
        var sun = Sun.At(Atmosphere.Earth, 30);
        Assert.Equal(sun.Illuminance / Sun.OutsideIlluminance, moon.Illuminance / Moon.OutsideIlluminance(0), 0.0001f);
        Assert.Equal(sun.Temperature, moon.Temperature, 1f);
    }

    [Fact]
    public void A_Moonless_Sky_Gives_Less_Than_A_Thousandth_Of_A_Lux() => Assert.InRange(Night.SkyIlluminance, 5e-4f, 1e-3f);
}
