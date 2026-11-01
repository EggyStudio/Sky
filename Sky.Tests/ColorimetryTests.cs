using Xunit;

namespace Sky.Tests;

public class ColorimetryTests
{
    [Theory]
    [InlineData(2_000, 25)]
    [InlineData(3_000, 25)]
    [InlineData(5_000, 25)]
    [InlineData(6_500, 25)]
    [InlineData(10_000, 150)]
    public void A_Black_Bodys_Color_Reads_Back_As_Its_Temperature(float kelvin, float within)
    {
        var color = Colorimetry.FromTemperature(kelvin);
        Assert.Equal(1, Colorimetry.Luminance(color), 0.001f);
        Assert.Equal(kelvin, Colorimetry.TemperatureOf(color), within);
    }
}
