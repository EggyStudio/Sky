# Sky

Sky is a library both engines, 3DEngine and BevyCSharp, take as a git submodule, for the sky's
clock, the sun's and the sky's light in physical units, the atmosphere's parameters, the weather and
the looks by the sun's height. Both engines light from its numbers while each draws its own sky. It
references no engine, and each engine implements the interfaces it declares where it binds the
module.

## What it holds

| Type | Gives |
|---|---|
| `Atmosphere` | The air as data, the planet's radius, the molecules' scattering, the aerosols' extinction and the ozone's absorption with their heights, the ground's albedo, and the transmittance along a way through it. `Atmosphere.Earth` is the Earth of Unity's HDRP, and `WithTurbidity` sets its haze |
| `Sun` | The sun's light outside the air, 128,000 lux at 5,772 K, and `Sun.At`, its lux, color and color temperature at the ground for its height over the horizon |
| `SkyLight` | The sky's luminance along a way and its illuminance on a level surface, the sun's light scattered once and more than once, in candelas a square meter and lux |
| `Moon` and `Night` | The moon's light by its phase and height, and a moonless sky's luminance and illuminance |
| `Colorimetry` | Linear sRGB's luminance and a black body's color, to and from its temperature |

Read against HDRP's Earth, the sun 54 degrees high reaches the ground at 110,000 lux and 5,026 K
and the sky lights a level surface with 7,860 lux beside the sun's 88,900 (`dotnet test Sky.Tests`).

## Taking it into an engine

An engine adds the repository as a git submodule under `modules/` and references its project, here
from a project one folder below the engine's root.

```bash
git submodule add https://github.com/EggyStudio/Sky.git modules/Sky
```

```xml
<ProjectReference Include="..\modules\Sky\Sky\Sky.csproj" />
```

A clone of the engine fetches the module with `git clone --recurse-submodules`, or with
`git submodule update --init` in a clone made without it, and a workflow checks it out with
`submodules: true` on `actions/checkout`. A change the engine uses is committed and pushed here
before the engine's commit that moves its pointer, so the engine's history never names a commit
this repository does not have.

## Building and testing

With the .NET 10 SDK:

```bash
dotnet build Sky.slnx
dotnet test Sky.Tests
```

## License

The Mozilla Public License 2.0, in [LICENSE](LICENSE).
