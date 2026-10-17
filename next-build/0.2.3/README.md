# Character Sheet for Risk of Rain 2 — isolated 0.2.3 next build

Original presentation-only explanations for Alexander's own character. This directory is an unstaged development candidate. Installed **0.2.2 remains unchanged** and awaits its first live test. No Unity/game test has been run.

The next build adds ordered contribution explanations to five authoritative live stat rows: health, damage, regeneration, armor and raw crit. It displays reviewed baseline/growth, known flat terms, additive pools, independent factors and conversions, followed by the signed unexplained residual. Matching a subtotal never declares complete attribution. Shield and barrier remain separate from health.

The common item registry expands from 23 to 37 partial definitions, each with current-stack and next-effective-copy component math and explicit conditions/luck scope. Eight existing buff names/states retain bounded glossary coverage; remaining timers are unavailable. Native Tab layout, key/cursor handling, pause ownership and hover attachment architecture are unchanged.

- [Exact coverage](docs/SourceCoverage.md)
- [Model stages, worked examples and unknowns](docs/CoreStatModel-0.2.3.md)
- [Build readiness and verification](docs/Readiness-0.2.3.md)
- [Development boundary](DEVELOPMENT-BOUNDARY.md)

## Build with existing tools and references

From this directory:

    .\build.ps1 -Mode CoreTests
    .\build.ps1 -Mode PluginReview -LoaderCorePath '..\..\.references\BepInExPack-5.4.2122'

471 meaningful synthetic assertions pass. The isolated plugin compiles to artifacts/review-native-tab/Alexander.CharacterSheet.dll. Build commands read existing game/BepInEx references without copying vendor files, installing software, deploying the DLL or launching the game. Deterministic compilation is enabled for this next build; identical input/source/path/environment builds are checked separately.

## Snapshot and runtime boundary

The source/build snapshot at ../../snapshots/installed-0.2.2-20261005T202447Z maps the frozen source, compiler and external reference hashes to installed 0.2.2 (FE8B5202… / 109,056 bytes). Its source ZIP includes no game/vendor binaries. The original 0.2.2 compiler invocation was nondeterministic, so that mapping does not claim bit-identical recompilation. The frozen BUILD-MAP.json retained here is a historical 0.2.2 record; this candidate's current evidence is in artifacts/next-build-verification.json.

This next build is **not installed**. Do not replace the profile DLL, change bootstrap/configuration, launch the game or redesign native UI before the parent/user's next authorization. The copied 0.2.2 readiness/test documents are historical references.

The five-stat model remains incomplete: Knurl/Slug raw regen scaling is deliberately omitted, Transcendence final health/shield prediction is unavailable, and unregistered item/buff/mod hooks remain in the signed residual. Attack/movement attribution remains the earlier baseline/growth explanation. No enemy/hidden loot/remote-player data or automation is added.
