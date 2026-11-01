# Review

Reviewed up to `c5ee6a9`, item 1, pushed at 14:15 on 2026-11-01, the documents as the other modules
keep them with the opening paragraph saying what this module is for; the commit keeps the form, the
three-mark subject, one sentence, the owner's identity as author and committer, no trailer (N 7.2
and N 7.6 kept at the first commit). After it `bff9e47`, item 2, pushed at 14:15, the skeleton as
Vegetation's with the name changed, the workflow's comment giving this machine's 4 s and 2 s, its
run green at 14:18 on the three systems in about a minute each, the repository's first. Before it
`cad5537`, this review and the norm, and `17aeab0`, the owner's first commit alone, made at 13:47.

## Now

The order the owner set on 2026-10-31 for the modules, this one holding the sky's clock, the sun's
and the sky's light in physical units, the atmosphere's parameters, the weather and the looks by the
sun's height, the model both engines light from while each draws its own sky. The documents first,
then the rest as the other projects have it, then the engines taking the module. Items 1 and 2 are
the game's session's, as for the six before; item 3 is in two parts by the session that owns each;
item 4 is each engine's session's. This module comes when 3DEngine's item 170 starts, which writes
its model here from the first commit rather than moving it later.

1. **The documents, one commit.** AGENTS.md on the game's shape (one paragraph saying what the
    module is and that no engine is referenced, N 2.1; the documents table with STYLE, COMMITS,
    REVIEW, NORM and DEVLOG; building and testing; Where things are with a row a folder; the
    conventions, the commit form among them), README.md (what the module is, how an engine takes it
    as a submodule, `dotnet build` and `dotnet test`), `.github/STYLE.md`, `COMMITS.md` and
    `CODE_OF_CONDUCT.md` as the other modules have them, `.gitignore` of the engines' shape with
    `/.github/dump/` in it, `.gitattributes`. Vegetation's documents are the models; nothing is
    written that the module does not have yet. Read by the reviewing session before it is pushed.

2. **The skeleton, one commit after item 1.** `Sky.slnx`, `Sky/Sky.csproj` as a class library on
    net10.0 with `Nullable` and `ImplicitUsings` in `Directory.Build.props` and
    ImportDirectoryBuildTargets false, `Sky.Tests/` on xunit with one test that the library loads,
    `build/version.txt` at `0.1` and `build/version.sh` as the engines', and
    `.github/workflows/build.yml` as Vegetation's, `test (os)` on ubuntu-24.04, windows-latest and
    macos-15 with `dotnet build Sky.slnx -warnaserror` and `dotnet test Sky.Tests --no-build`, a
    time limit on every job with its reason, `permissions` read and the engines' concurrency group.
    Green on the three systems before item 3.

3. **The model, in two parts.** The first part is BevyCSharp's SkyClock, the sun, the moon and the
    stars by spherical astronomy for an hour, a day and a latitude, WeatherState, the conditions
    easing toward a target or a forecast, and LookCycle, the looks keyed by the sun's height, which
    reference no engine and move as they are with their tests, by the BevyCSharp session after its
    items 90 and 68. The second part is what 3DEngine's item 170 needs and writes here from the
    start, by the 3DEngine session, the sun's illuminance in lux and color temperature by its
    elevation and the air's turbidity, the sky's luminance on the same scale, the moon's and the
    night's, and the atmosphere's parameters as data (the planet's radii, Rayleigh, Mie and ozone),
    read against the reference scene's figures, 97,050 lux at 5,633 K with the sun 54 degrees high.
    Each engine draws its own sky from the module's numbers, BevyCSharp through Bevy's atmosphere,
    3DEngine through item 170's pass, so the two light from one sun and one sky. One commit a part,
    the strict build and the suite green, a Shared line in the engines' REVIEW.md when a part lands.

4. **The engines take the module.** Each engine adds this repository as a git submodule under
    `modules/` and references its project, its workflows checking submodules out, its own clock,
    light and weather code becoming the binding of the module's model (N 2.1) and the rest deleted;
    a change used by an engine is committed here first and pushed before the engine's commit that
    moves the pointer. One commit an engine, after item 3's part that engine uses.

## Verdicts

None yet.

## Decisions

1. **The repository's birth.** The owner made this repository on 2026-11-01 at 13:47, the seventh
    module beside Modeling, Vegetation, Levels, Landscape, RadianceCascades and Tools, after asking
    at 13:4x whether the sky, the atmosphere and the weather belong in a repository of their own so
    both engines use them, or whether BevyCSharp keeps Bevy's sky and 3DEngine builds its own, and
    reading the answer that the model is shared and the drawing is each engine's. The order is the
    modules', the documents first, then the rest as the other projects have it, then the engines
    taking the module.

2. **A git submodule, not a package.** The owner chose on 2026-10-31 that the engines take a module
    as a git submodule built as a project of their own, since a package's upload, wait, test and
    pack is slower to iterate than a commit. A change is committed here first; the reviewing session
    pushes this repository before the engine's commit that moves its pointer; the workflows check
    submodules out. The game keeps using the engine through the engine's package.

3. **The module knows no engine.** N 2.1. The module references no engine, package or bridge; what
    it needs of a renderer, a world, a scheduler or a file system it takes through interfaces it
    declares, which each engine implements where it binds the module. A type of an engine's in this
    repository is a fault.

4. **The repository's terms are the owner's.** The repository was made by the owner with its terms
    file in place, which stands as the owner set it and is spoken of nowhere else.

5. **Who writes here.** The game's session writes items 1 and 2, as for the six modules before; the
    BevyCSharp session moves its clock, weather and looks (item 3's first part) and the 3DEngine
    session writes the sun's and the sky's light and the atmosphere's parameters here as its item
    170 begins (item 3's second part); each engine's session binds the module (item 4). Every
    session reads REVIEW.md's head, its items and the Replies before a batch, as N 7.7 says, and the
    reviewing session pushes.

6. **The model is shared, the drawing is each engine's.** The sky's clock, the sun's and the sky's
    light in lux and kelvin, the atmosphere's parameters, the weather and the looks by the sun's
    height are arithmetic and data no engine owns, so both engines light from one set of numbers and
    the reference scene's figures come out of the same astronomy in both. The drawing is not shared,
    since Bevy's atmosphere is a component of its renderer in WGSL and 3DEngine's pass is Slang over
    Vulkan, and nothing is gained by moving either; BevyCSharp keeps Bevy's atmosphere, fog and
    clouds, 3DEngine builds its pass under item 170, and each feeds its sky from the module. Clouds'
    drawing and rain stay with each engine and come after the global illumination items (3DEngine
    Decision 79).

## Replies

Item 3's second part, from 3DEngine's item 170, is in the commit after `bff9e47`. It holds
`Atmosphere` with HDRP's Earth as data and turbidity as 1 plus the aerosols' optical depth over the
molecules' at 550 nm, `Sun.At` (lux, color, kelvin), `SkyLight` (Hillaire's single and multiple
scattering in candelas a square meter and lux), `Moon`, `Night` and `Colorimetry`, with 20 tests.
Read against the scene in HDRP, the transmittance at 53.97 degrees is 0.926, 0.850 and 0.710 here
against 0.925, 0.849 and 0.710 by HDRP's own formula. The sun reaches the ground at 109,960 lux and
5,026 K here, where the scene's light, 97,050 lux at 5,633 K outside the air, reaches 73,900 lux at
4,924 K through HDRP's transmittance, which its light loop multiplies the light by. The color holds
within a hundred kelvin, and the lux is two thirds of the sun's here, since the scene sets 120,000
lux times the sine of the sun's height outside the air. Its sky volume's intensity mode is 2,
Multiplier in HDRP's order of Exposure, Lux and Multiplier, at 1, so its 20,000 lux is never read,
and its sky is the atmosphere's lit by that light, 7,860 lux on a level surface at 54 degrees here
for a sun of 128,000. The scene's TimeOfDay puts the sun at 16.55, 51.40 and 53.97 degrees at 8:00,
12:00 and 15:05 on 27 June of its year 75.
