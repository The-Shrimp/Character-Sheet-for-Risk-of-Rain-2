# Native tooltip extension and simple stats: 0.2.2.5

This isolated source-only successor follows 0.2.2.4 and the user's latest Total / Base / Bonuses request. Nothing is deployed. The actual host Dev profile remains 0.2.2.2.

The ordinary Health, Damage, Regeneration and Armor hover shows only:

- Total: the live CharacterBody value.
- Base: the actual base plus the actual growth coefficient per level.
- Bonuses: known nonzero flat sources first, then percentage modifiers; an unaccounted final difference uses Other/unknown.

Examples checked by the pure-source suite:

    Level 1 health
    Total: 110
    Base: 110 + 33 per level
    Bonuses: None

    Level 4 health
    Total: 209
    Base: 110 + 33 per level
    Bonuses: None

    Level 1 damage
    Total: 12
    Base: 12 + 2.4 per level
    Bonuses: None

    Level 4 health with an unaccounted +20
    Total: 229
    Base: 110 + 33 per level
    Bonuses:
    Other/unknown: +20

A supplied negative modifier displays -20%; a factor of 1.1 displays +10%. Known late flat sources still display with the other flat sources, but the calculation list remains in its original order. Optional clickable Details shows the simple summary first and the ordered before/after math and diagnostics beneath. No Shift handler or hint is present. Level 4 applies 33 times (4 minus 1) once; the resulting +99 belongs in Details rather than being mislabeled as the base or per-level coefficient.

The same simple format also applies to crit, attack speed and movement speed. Shield/barrier keeps its existing separate-pool presentation.

## Native items

The predecessor native pointer-enter/exit contracts, original title/body preservation and native style capture are unchanged. The native game tooltip remains the actual tooltip. The owned addition has no item header or repeated native description and stays beneath the existing tooltip at its width.

Source inspection found that 0.2.2.4 bound only own scoreboard icons. This successor also binds the local HUD's public itemInventoryDisplay, guarded by public HUD.targetMaster matching the local player's master and by exact display/HUD ancestry. Other-player strips and foreign spectating targets are excluded. Both native icon pathways route through the same preserved native tooltip contract. The public compile-only probe confirms those fields; no game or probe implementation was executed.

The owned expanded item rows still use their own native TooltipProvider with title/body/colors copied from the corresponding own native provider. Existing native providers are read only.

When no quantitative component is available, the added body is empty and the Details control is hidden. Unknown formula plus unknown luck yields the original native tooltip alone. A known scoped luck note can retain only the small clover. The screenshot's Predatory Instincts has no registered formula here, so it does not receive an unavailable-formula popup. Available current-stack, next-copy and conditional context remain optional clickable Details. Missing next-copy values are omitted rather than replaced with a filler line.

## Scope and evidence

The parent visually inspected the three supplied Library screenshots and reported the old technical Damage hover, the ordinary native top HUD Predatory Instincts tooltip and the installed replacement scoreboard tooltip. These screenshots establish the design correction, not a runtime result for this undeployed candidate. No additional Library materialization was needed.

Attribution scope is unchanged: actual baseline, actual growth and the existing crit-glasses flat adapter. This change does not claim complete health/damage/regen/armor source attribution, add new stage assumptions or import future feature branches. Other/unknown is a residual after the known operations, not a newly inferred early flat effect. None means no displayed nonzero known bonus or residual; it does not certify complete source coverage. Optional Details retains that distinction.

The source remains within original project code and compiler checks of public API references. No further proprietary implementation inspection, IL/decompilation or private-field access was performed.

## Validation and remaining runtime work

8812 assertions pass, including 24 combinations of level 1/4, health/damage/regen/armor basis and positive/negative/zero final differences. Tests cover growth exactly once, base coefficients, neutral bonuses, negative modifiers, conversion ordering, unknown sources, omission of unsupported item filler and ownership of both native item surfaces. The inherited 24 Tab sessions by 16 item transfer tests, 36 add-on placement cases, 96 sizing cases and lifecycle/pause tests still pass. Only superseded tooltip text expectations and one suite call were updated.

The plugin and public inventory API probe compile without warnings. The probe was not executed. Calculation engine, mathematical definitions, native pointer routing, Details leases, safe HUD layer, pause/session guards and docking geometry remain unchanged. Plugin control source differs only in its two version strings.

Live checks remain necessary for native tooltip style/draw order, both surface identities, pointer movement to Details/clover, source recycling, Tab cleanup and multiplayer gameplay-click suppression. Augmentation exists only during the established native Tab session. Outside that session the existing native tooltip behavior is retained. The owned extension still pins the first observed native bounds; it declines when there is no safe room beneath the native tooltip.

No game launch/close/input, profile/game/settings change, deployment or publishing occurred.

## Candidate

Parent DLL SHA256: 08A4E34C4F0AF33DC40505077606BDA619A28B9E450CA6CDCF25433B5FD788DD

Candidate DLL: artifacts/review-native-tab/Alexander.CharacterSheet.dll
Bytes: 142848
SHA256: 257EBD7A6B913F6CFF0EE1362106A14B43675FF036361DB1E43B05735E174820

Installed actual host DLL: 0.2.2.2, SHA256 6C54A2FACA7A9751DD240EF3514647347C4B2BD86660378177C11F662964A396
Exactly one custom plugin DLL and all nine supporting/configuration files match the recorded baseline. Both frozen next-feature DLLs and all predecessor source/test files remain unchanged.

See artifacts/SIMPLE-STATS-VERIFICATION.json for the complete hash record and artifacts/SIMPLE-STATS.diff for the reviewable source/test changes. Deployment requires a separate explicit instruction.
