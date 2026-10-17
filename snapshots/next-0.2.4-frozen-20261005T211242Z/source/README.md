# Character Sheet for Risk of Rain 2 — isolated 0.2.4 robustness patch

This separate source/build candidate preserves installed **0.2.2** and reported **0.2.3**. It is not deployed and has no game/runtime validation.

The patch improves uncertainty handling in the existing five-stat model:

- Short regen hovers name omitted Knurl/Slug base-rate terms. Unknown Slug activation remains explicit.
- Protection hovers show current/max HP, current/max shield and barrier as separate observations. No converted-health source total or shield partition is inferred.
- Missing live stat observations remain unavailable; missing master luck no longer defaults to zero.

No exact regen-placement or Transcendence-conversion formula is added. All source attribution stays incomplete, and signed residuals retain their meaning. The inherited 37 partial item definitions and eight bounded buff states are unchanged.

**476 synthetic assertions pass**, and the isolated plugin compiles against existing local references. Native UI architecture is unchanged.

- [Patch readiness and frozen-build verification](docs/Readiness-0.2.4.md)
- [Stat model, examples and public-source limits](docs/CoreStatModel-0.2.4.md)
- [Exact component coverage](docs/SourceCoverage.md)
- [Frozen 0.2.3 location](artifacts/frozen-0.2.3-location.json)
- [Development boundary](DEVELOPMENT-BOUNDARY.md)

Build from this directory:

    .\build.ps1 -Mode CoreTests
    .\build.ps1 -Mode PluginReview -LoaderCorePath '..\..\.references\BepInExPack-5.4.2122'

Output: artifacts/review-native-tab/Alexander.CharacterSheet.dll. Existing game/loader references remain external. No vendor binaries are copied and the build script does not install, deploy or launch anything.

Installed 0.2.2 is still the first live-test candidate. Reported 0.2.3 is frozen with the original 679D985D… DLL hash and exact source hashes, plus a source-only ZIP and archived copy of our own DLL. Copied older readiness/model documents are historical references; current patch status is linked above.
