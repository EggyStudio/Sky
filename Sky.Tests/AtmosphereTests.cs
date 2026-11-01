using System.Numerics;
using Xunit;

namespace Sky.Tests;

public class AtmosphereTests
{
    private static readonly Atmosphere Earth = Atmosphere.Earth;

    private static Vector3 Depth(Vector3 transmittance) => new(-MathF.Log(transmittance.X), -MathF.Log(transmittance.Y), -MathF.Log(transmittance.Z));

    [Fact]
    public void The_Optical_Depth_Straight_Up_Is_Each_Layers_Coefficient_Times_Its_Depth()
    {
        // Each exponential layer holds its sea level's coefficient over its scale height, cut at
        // the
        // top, and the ozone's layer its middle's over half its width.
        var rayleigh = Earth.RayleighScattering * Earth.RayleighScaleHeight * (1 - MathF.Exp(-Earth.Top / Earth.RayleighScaleHeight));
        var aerosol = Earth.AerosolExtinction * Earth.AerosolScaleHeight * (1 - MathF.Exp(-Earth.Top / Earth.AerosolScaleHeight));
        var expected = rayleigh + new Vector3(aerosol) + Earth.OzoneAbsorption * Earth.OzoneWidth / 2;
        var depth = Depth(Earth.Transmittance(0, 1));
        for (int i = 0; i < 3; i++)
            Assert.InRange(depth[i] / expected[i], 0.995f, 1.005f);
    }

    [Fact]
    public void The_Sun_At_54_Degrees_Comes_Through_As_HDRPs_Earth_Lets_It()
    {
        // HDRP's own transmittance for its Earth, Chapman's function for the two exponential layers
        // and its ozone's sum, from the ground with the sun 53.97 degrees high, 0.925, 0.849
        // and 0.710.
        var through = Earth.Transmittance(0, MathF.Sin(float.DegreesToRadians(53.97f)));
        Assert.Equal(0.925f, through.X, 0.004f);
        Assert.Equal(0.849f, through.Y, 0.004f);
        Assert.Equal(0.710f, through.Z, 0.004f);
    }

    [Fact]
    public void A_Way_Into_The_Ground_Lets_Nothing_Through_And_One_Over_The_Horizon_From_Above_Does()
    {
        Assert.Equal(Vector3.Zero, Earth.Transmittance(0, -0.05f));
        // From 10 km the horizon lies 3.2 degrees below level, so a way 1 degree below leaves the
        // air.
        var below = Earth.Transmittance(10_000, MathF.Sin(float.DegreesToRadians(-1)));
        Assert.True(below.X > 0.1f, $"red through {below.X}");
    }

    [Fact]
    public void Turbidity_Sets_The_Aerosols_And_Reads_Back()
    {
        Assert.Equal(1.111f, Earth.Turbidity, 0.001f);
        Assert.Equal(3, Earth.WithTurbidity(3).Turbidity, 0.0001f);
        Assert.Equal(1, Earth.WithTurbidity(0.5f).Turbidity, 0.0001f);
        Assert.Equal(0, Earth.WithTurbidity(1).AerosolExtinction);
    }
}
