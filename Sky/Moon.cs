namespace Sky;

/// <summary>
/// The moon's light outside the air by its phase and where it reaches the ground through it.
/// </summary>
/// <remarks>
/// The moon's illuminance outside the air follows Krisciunas and Schaefer's fit of the moon's
/// brightness by its phase angle, the angle at the moon between the sun and the eye, 0 when full
/// and 180 when new, which falls faster than the lit share of the disc since the moon's rough
/// surface throws shadows that the full moon alone shows none of. Its light is the sun's reflected,
/// in the sun's color, the slight redness the moon's albedo gives it left out.
/// </remarks>
public static class Moon
{
    /// <summary>
    /// The moon's illuminance outside the air in lux on a surface facing it, at a phase angle of
    /// <paramref name="phaseAngle"/> degrees, about 0.31 when full.
    /// </summary>
    public static float OutsideIlluminance(float phaseAngle)
    {
        // Their fit gives foot-candles, 10.764 lux each.
        var angle = Math.Clamp(MathF.Abs(phaseAngle), 0, 180);
        return 10.764f * MathF.Pow(10, -0.4f * (3.84f + 0.026f * angle + 4e-9f * MathF.Pow(angle, 4)));
    }

    /// <summary>
    /// The moon's light at <paramref name="altitude"/> meters through <paramref name="air"/>, the
    /// moon <paramref name="elevation"/> degrees over the horizon there at a phase angle of
    /// <paramref name="phaseAngle"/> degrees.
    /// </summary>
    public static CelestialLight At(Atmosphere air, float elevation, float phaseAngle, float altitude = 0) =>
        Sun.Through(air, Sun.Outside / Sun.OutsideIlluminance * OutsideIlluminance(phaseAngle), elevation, altitude);
}

/// <summary>
/// The light of a clear night with no moon, the stars' and the air's own glow, away from a town's.
/// </summary>
public static class Night
{
    /// <summary>
    /// The sky's luminance in candelas a square meter, 2.26e-4, a dark site's sky of 21.7
    /// magnitudes a square arcsecond in the visual band.
    /// </summary>
    /// <remarks>
    /// A sky of m magnitudes a square arcsecond has a luminance of 108,000 times 10 to the power of
    /// -0.4 m candelas a square meter.
    /// </remarks>
    public const float SkyLuminance = 2.26e-4f;

    /// <summary>
    /// The illuminance the sky gives a level surface in lux, pi times <see cref="SkyLuminance"/>,
    /// the sky taken as alike over its whole dome.
    /// </summary>
    public const float SkyIlluminance = MathF.PI * SkyLuminance;
}
