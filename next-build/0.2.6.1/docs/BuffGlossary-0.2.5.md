# Buff glossary — isolated 0.2.5

19 named states are registered. [Exact identity inventory](../artifacts/buff-source-inventory.json) records each original effect reference, count meaning, conditions, public source/version and confidence. [Coverage table](SourceCoverage.md) provides the concise list.

The named identity/effect reference is the [LookingGlass public BuffDefinitions snapshot](https://github.com/Wet-Boys/LookingGlass/blob/4aa6dff73e9df72a50a67b9d9cf449bd6dcaf9cf/LookingGlass/BuffDescriptions/BuffDefinitions.cs), pinned at 4aa6dff73e9df72a50a67b9d9cf449bd6dcaf9cf, LGPL-3.0. Original glossary/metadata logic and wording were authored here; no external implementation/binary was copied.

Ready and cooldown states are separate references. Raincoat ready/recharge, Safer Spaces ready/cooldown and Fin ready/cooldown do not share an inferred effect or timer. State counts are distinguished from charges, stored level gains, Nectar type counters and active stat stacks. Fixed War Horn/Warbanner references do not multiply with the observed count. Opal/Red Whip status counts do not substitute for effective item copies. Medkit pending does not imply restored HP or raw regen.

Runtime dispatch uses public object identity. Unfamiliar visible definitions retain internal IDs and explicit effect-unknown labels, including similar names. Hidden or unknown visibility is filtered; broad invulnerability is not guessed.

Nectar uses observed metadata rather than registry labels. Its three outcomes are eligible, not counted and unknown. Known exclusions remain decisive when other metadata is missing; otherwise missing context is unknown. Qualifying stacks count as one type, and the observed Nectar counter excludes itself. The own live game counter remains authoritative.

All remaining timers are unavailable. Source nominal durations are labeled separately. No private per-stack timer access or multiplayer countdown guarantee is introduced.

525 synthetic assertions pass; public-field compilation succeeds against existing references. Tests cover zero/invalid/missing counts, 1/4/10 active stat stacks, two Faraday components, fixed magnitudes, ready/recharge transitions, hidden filtering, unfamiliar-name fallback and eligibility changes driven by flags. Unity dispatch, real buffs and UI presentation still require runtime testing.

No native layout/input/cursor/pause/hover architecture is changed. Installed 0.2.2 is still the first user-test candidate; reported 0.2.3/0.2.4 and this separate candidate are not automatically installed.
