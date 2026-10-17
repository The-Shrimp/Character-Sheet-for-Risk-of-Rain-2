# Character Sheet hierarchy: source-only 0.3.0

This candidate follows installed 0.2.2.5. It is isolated under hierarchy-build/0.3.0 and has not been deployed or tested in game. Installed DLL, supporting/configuration files and frozen next-feature candidates are preserved.

## Presentation

Character Sheet is the fixed title. Base Stats and its Defense/Offense/Movement/Luck groups start expanded. Conditional sections and the retained other-items/buff glossary start collapsed.

    Character Sheet
      Base Stats
        Defense [shield]
          Health
          Shield (only when maximum capacity exists)
          Regen
          Armor
          Dmg Reduction
        Offense [sword]
          Damage
          Attack Speed
          Cooldown Reduction
          Crit Chance
        Movement [running shoe]
          Movement Speed
          Sprint Speed Modifier %
          Maximum jumps
        Luck [clover]
          Luck +/- value
      General Damage Modifiers
      On-Hit [impact burst]
      On-Kill [skull]
      Out of Combat
      While Sprinting
      Other items & buffs

Stat rows display current totals. Hover/focus retains Total / Base / Bonuses, with clickable Details for ordered calculation and diagnostics. Units, signed luck and the maximum-jump label make the meaning explicit without expanding every row. Other/unknown remains the short label for unattributed differences.

The card's background Image color alpha is 0.8. Text and icon colors retain alpha 1; row and viewport backgrounds are clear, and no CanvasGroup alpha is added. Native item tooltip styles and legible hover backgrounds are preserved. The native menu's blur is outside this change.

All margin glyphs are original runtime vector graphics. Item and passive icons use their existing publicly accessible native sprite references; no extracted asset files or external asset licenses are introduced.

## Defense and totals

Shield visibility uses actual maximum shield capacity, so depletion does not hide a shield the player still has. The row displays capacity; current shield is in Details. Temporary barrier has a separate entry under Other items & buffs.

Armor displays the raw live stat. Hover adds the existing armor-derived reduction formula, including honest negative reduction for negative armor. Dmg Reduction is a separate maintained Repulsion Armor Plate component (5 per effective copy), with eligible-hit and minimum-damage caveats. It is not substituted with armor percentage or presented as every possible modded protection source.

Health, damage, regen, armor, attack speed, crit and movement retain live CharacterBody totals. New public API compile checks support base/maximum jump count, sprint multiplier, shield baseline/growth/capacity, danger/combat state and enabled passive metadata. Luck reads signed master.luck. Maximum jumps means total available jump count, not the current number remaining.

The sprint modifier displays (live sprint multiplier - 1) times 100. Its survivor base coefficient and complete source attribution remain unknown.

General cooldown reduction deliberately remains Unknown in the row and simple hover. The candidate does not infer a common value from GenericSkill slots, which could include Hardlight Afterburner or other skill-specific effects. Optional Details lists separate shared-item references for Alien Head, Light Flux and Purity. Percent and flat-second references are not combined into a fabricated universal total. These reference facts were checked against [the public definition snapshot](https://github.com/Wet-Boys/LookingGlass/blob/4aa6dff73e9df72a50a67b9d9cf449bd6dcaf9cf/LookingGlass/ItemStats/ItemDefinitions.cs) and [its mathematical helper](https://github.com/Wet-Boys/LookingGlass/blob/4aa6dff73e9df72a50a67b9d9cf449bd6dcaf9cf/LookingGlass/Utils.cs). No third-party implementation was copied into the candidate.

## Effect scope

There are 44 explicit item routing definitions. Crowbar, Delicate Watch and Focus Crystal are separate conditional rows; their modifiers are not summed into universal damage. Unregistered visible items remain available through Other items & buffs and their actual native tooltips.

On-Hit includes explicit item effects and recognized enabled native passive metadata only. Active ability data is never enumerated for effects, and ability-specific ignite is excluded. The passive-token routing currently covers Bandit/Acrid/REX/Railgunner tokens; unrecognized or modded passives remain under Other with their native description. This is categorical routing, not a new passive formula engine.

ATG, Tri-Tip and Ukulele have concise attack-proc scope notes. Plasma Shrimp separately states shield/nonzero-proc eligibility and does not inherit a random trigger chance. Quantitative formulas remain conditional references in optional Details; no single current attack proc coefficient is fabricated.

Gasoline and Will-o'-the-wisp use On-Kill, as explicitly approved. Cautious Slug's note uses actual out-of-danger state and distinguishes it from out-of-combat state. Red Whip uses actual out-of-combat state. Wax Quail requires a jump while sprinting; Rose Buckler uses merely sprinting. Relevant conditions stay in hover content rather than bloating the collapsed row.

Item rows show one name and current stack count. Identity-based deduplication keeps one row per item without suppressing distinct passive or buff sources. Existing quantitative coverage remains 23 registered item definitions; the routing count does not imply all 44 have new current/next-copy formulas.

## Collapse, focus and lifecycle

Expansion preferences persist in memory across Tab sessions, without configuration writes. Collapsing a parent preserves subgroup choices. Controls retain stable node identities, and focus recovery chooses a visible owned ancestor.

The fixed header remains above the bounded vertical viewport. Native UI buttons provide explicit Up/Down navigation; Left/Right collapse/expand headers, Left from a row selects its parent, and Submit toggles Details on a focused row. Focus scrolls into view. Icons are redundant with text, and a visible focus marker distinguishes selection. Runtime validation of the actual native keyboard/controller entry path remains necessary; no operating-system screen-reader integration is claimed.

Focused item rows route through the public native tooltip pointer contract in the actual MP UI context. Original native providers are not overwritten. Owned focus tooltip cleanup checks exact context/provider identity and releases its lease during hide/disposal, with binding teardown in finally blocks.

The own scoreboard and top HUD inventory ownership/binding paths are preserved from 0.2.2.5. Native title/body/style/anchor code and public pointer patches are preserved. New short condition/proc notes extend the original tooltip; unsupported formulas produce no unavailable-formula filler. Note-only additions omit empty footer controls and require a readable minimum viewport; lack of room declines the addition.

The existing safe HUD branch, held-Info guard, session and pause lease are preserved. Plugin changes beyond its two version strings are only the in-memory expansion state and its constructor argument. No Shift handler, raw keyboard polling, native root reparenting, gameplay stat writes or active ability effect hooks are added.

## Validation

9928 pure-source assertions pass, including 1116 added assertions. New coverage includes 44 item routes, ability/unknown-origin exclusion, proc distinctions, duplicate identities, corrected Defense math, shared-CDR reference separation, 128 collapse-state combinations and 96 hierarchy sizing cases. Keyboard focus retention, idempotent Details expansion, focus recovery and scroll bounds are checked.

All inherited test files are unchanged except adding one suite call in CoreTests. Existing 24 Tab sessions by 16 transfer items, 36 tooltip placement cases, 96 tooltip sizing cases, simple stat formatting and lifecycle/pause/math tests still pass. Plugin and public API probe compile without warnings; the probe was not executed.

The source review confirms unchanged calculation engine, established formulas, simple stat renderer, native tooltip style/anchor, safe HUD layer, native pointer hook implementation and both inventory ownership/binding paths. No 0.2.6.1 or other frozen feature branch was merged.

Installed 0.2.2.5 and all nine support/configuration hashes match the source-work baseline. Before this work, the own config generated version comment changed from v0.2.2.2 to v0.2.2.5; reversing only that marker reproduces the deployment-time hash. No setting values or configuration bytes were written by tools.

## Screenshot and runtime limits

Library image reads returned metadata/descriptions and asset pointers, without ImageContent pixels. The current supported materialization helper then failed while applying required metadata on Windows:

    AttributeError: module 'os' has no attribute 'setxattr'

No screenshot pixels were inspected or used as evidence of this undeployed candidate. The implementation follows the explicit written hierarchy/corrections and original source.

The [Library SKILL.md](skill://plugin_connector_1p_1b8ff8edfc1481918b252c8277e23125/library/SKILL.md) requires preserving Library identity; its [materialization guidance](skill://plugin_connector_1p_1b8ff8edfc1481918b252c8277e23125/library/references/materialization.md) says: “If the helper is unavailable, stop rather than producing a local file without its Library metadata.” The failing Windows metadata step is treated as an unavailable materialization route, so no metadata-free bypass was performed.

Live checks remain necessary for native style/draw order, icon clarity, label clipping, 80% background appearance, keyboard/controller focus entry, scroll-to-focus, shield capacity transitions, native focus tooltip cleanup, both inventory hover pathways, Tab/pause cleanup and multiplayer gameplay-click routing. The first-observed native tooltip bounds pin is retained.

General CDR, complete per-source stat attribution, sprint base coefficient, arbitrary modded flat reduction and unrecognized passive classification remain coverage gaps. No game launch/close/input, game/profile write, settings change, deployment, installation or publishing occurred.

## Candidate

DLL: artifacts/review-native-tab/Alexander.CharacterSheet.dll
Bytes: 167936
SHA256: 9EB121AD7C2808645258A2EF5359CDE538F5E0C9BEA7FD3C8DF84C00B78750C4
Parent 0.2.2.5 SHA256: 257EBD7A6B913F6CFF0EE1362106A14B43675FF036361DB1E43B05735E174820

Review artifacts/HIERARCHY.diff and artifacts/HIERARCHY-VERIFICATION.json. Deployment requires later parent/user coordination and a fresh closed-game check.
