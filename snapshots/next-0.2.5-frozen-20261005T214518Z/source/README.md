# Character Sheet for Risk of Rain 2 — isolated 0.2.5 glossary expansion

This separate source/build candidate adds 19 bounded named buff-state references, live metadata-based Nectar assessment and an explicit unfamiliar-buff fallback. Installed **0.2.2** and reported **0.2.3/0.2.4** remain frozen. The user should test stable 0.2.2 first.

Ready states, cooldown states, charges, stored-level counters, Nectar type counters and active stat stacks have distinct meanings. Known descriptions carry original effect references, conditions and pinned public source/version confidence. Every remaining timer stays unavailable.

Public object identity selects known descriptions; a familiar internal name alone cannot select a confident effect. Unfamiliar visible states show their internal ID and unknown effect. Hidden/unknown visibility is filtered.

Nectar has eligible, not-counted and unknown outcomes from actual count/metadata. Registry labels, cooldown/debuff semantics, names, icons and stack size do not override live flags. Own game counters remain authoritative.

**525 synthetic assertions pass** and the isolated plugin compiles against the existing local references. Native UI architecture and inherited stat/item math are unchanged. No runtime test, deployment, installation, profile/game/bootstrap edit or power/security change occurs.

- [Readiness and frozen hash verification](docs/Readiness-0.2.5.md)
- [Glossary behavior and source limits](docs/BuffGlossary-0.2.5.md)
- [Exact source/confidence inventory](artifacts/buff-source-inventory.json)
- [Exact item/buff coverage](docs/SourceCoverage.md)
- [Frozen 0.2.4 location](artifacts/frozen-0.2.4-location.json)
- [Development boundary](DEVELOPMENT-BOUNDARY.md)

Build from this directory:

    .\build.ps1 -Mode CoreTests
    .\build.ps1 -Mode PluginReview -LoaderCorePath '..\..\.references\BepInExPack-5.4.2122'

Output: artifacts/review-native-tab/Alexander.CharacterSheet.dll. Existing external game/BepInEx/Harmony references are not copied. Build scripts do not deploy or launch anything. Copied earlier readiness/model documents are historical references.
