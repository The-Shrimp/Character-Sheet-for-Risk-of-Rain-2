# Ordered five-stat contribution model — 0.2.4

This is an original explanation model wired to own live public CharacterBody/Inventory observations. It does not recalculate gameplay stats, inspect game method implementations, or provide complete attribution. ContributionResult.Live is independent from modeled inputs and remains authoritative; no residual is fed back into an invented source.

Every result retains an ordered stage trace, reviewed subtotal, signed live - modeled residual where a final comparison is supported, condition/count notes, and incomplete/unknown labels. A zero residual or a match within tolerance never certifies completeness. Missing/nonfinite baseline/level inputs or overflow suppress prediction. Display formatting preserves the actual residual even inside tolerance (1e-5 + 1e-6 times the larger magnitude).

## Registered stage representation

n is observed effective count, not permanent count. Missing/negative effective counts produce uncertainty, never a permanent-count fallback. Temporary effective copies count; zero-effective owned items are inactive. g = max(0, observed level - 1).

| Stat | Reviewed ordered terms | Explicit remaining unknowns |
|---|---|---|
| Maximum health | observed survivor base + level growth; 25n Steak, 40n Knurl, observed stored Infusion points; Pearl/Irradiant health contributions combined in one additive pool; observed active Nectar factor; observed combined curse divisor once | Floors, cached payouts, other health sources/conversions/hooks; Transcendence final HP/shield conversion unavailable |
| Raw damage | observed base + level growth; Irradiant additive pool; independent Glass factor 2^n; observed active Nectar factor | Other stat sources/hooks; Watch/Crowbar/AP Rounds/Focus Crystal/proc missiles/explosions are hit context and excluded |
| Raw regeneration | observed base + level growth; Knurl/Slug only with an explicitly supplied raw-stat scaling input; Irradiant pool; active Nectar | Runtime deliberately leaves raw item scaling unknown, showing Knurl 1.6n /s and active Slug 3n /s base references as omitted; no healing-efficiency inference |
| Raw armor | observed base + level growth; early Irradiant factor; late Buckler 30n while sprinting, Opal 100n while ready buff active; late Nectar | Other early/late sources/hooks; Plate flat reduction is not armor |
| Raw crit points | observed base + level growth; Glasses 10n, Irradiant 10n; fixed first-copy +5 each Predatory/Scythe/Shatterspleen; active FullCrit +100; Lost Seer's conversion to zero; late Nectar | Other points/conversions/hooks; standard probability cap/luck and crit damage are separate |

The order is a reviewed public-source reference, not an audited complete mirror of the installed game's calculation. Stage/API documentation uses R2API's approximate stat-pool descriptions; the model also preserves order facts established in the prior project review. No new game implementation inspection was used for this work.

Health's observed combined curse is applied once; Glass's item-health reference is not applied again. Active Transcendence makes modeled final health and residual unavailable, while preserving a labeled pre-conversion subtotal. Current/max HP, current/max shield and barrier are shown separately, without adding them into maximum health.

The stat model uses the observed active Nectar counter directly, not assumed full copy capacity. Inactive/cached Nectar is omitted; a positive effective item and active buff are required. The item tooltip's fixed-type current/next capacity comparison is a separate conditional model and may differ from a mod-altered live counter.

## Tested numerical examples

These are synthetic test inputs, not captured game data.

Health at level 3: base100 + growth60 + Steak75 + Knurl40 + observed Infusion20 =295. Pearl2 and Irradiant1 share one additive pool: 295 x1.3 =383.5. Active Nectar4 gives444.86. Combined curse divisor2 gives **222.43**. Live227.43 produces **+5** residual; live217.43 produces **-5**. Adding Glass copies does not duplicate the already observed curse.

Damage: (base12 + growth4.8) x1.2 Irradiant x4 Glass x1.16 Nectar = **93.5424**. Seven conditional-hit/proc item families are expressly excluded from this raw-stat subtotal.

Armor: base20 x1.1 Irradiant +60 sprint Buckler +100 ready Opal =182; late Nectar x1.16 = **211.12**. When conditions are inactive, the flat terms vanish; unknown conditions are labeled and omitted.

Crit: base1 +110 Glasses +20 Irradiant +5 each Predatory/Scythe/Shatterspleen = **146 raw points**, retained above100. FullCrit makes246; Lost Seer's conversion makes0; later Nectar keeps0. Raw points are distinct from luck-adjusted capped probability.

Regen: with explicitly supplied synthetic raw-item scale1.6, level4 base/growth1.6 + Knurl3.2 x1.6, then Irradiant1.1 and Nectar1.16 gives **8.57472 /s**. With the runtime's unknown scaling, the Knurl term is omitted and reviewed subtotal is **2.0416 /s**, with the uncertainty and residual preserved.

A Transcendence example with HP1, shield150, barrier30 retains these separate pools and shows final-health prediction unavailable. It never reports181 maximum health.

## Source and coverage limits

Public component fact snapshot: [LookingGlass ItemDefinitions at 4aa6dff](https://github.com/Wet-Boys/LookingGlass/blob/4aa6dff73e9df72a50a67b9d9cf449bd6dcaf9cf/LookingGlass/ItemStats/ItemDefinitions.cs), LGPL-3.0. Original expressions/logic were authored here; no LookingGlass implementation or binary is included.

Public stage/pool descriptions: [R2API RecalculateStats API documentation](https://github.com/risk-of-thunder/R2API/blob/master/R2API.RecalculateStats/RecalculateStatsAPI.cs), reviewed 2026-10-05; moving master reference, approximate formulas, not an installed-build verification.

Effective count semantics: [R2API core API 1.4 changelog](https://risk-of-thunder.github.io/R2Wiki/Mod-Creation/Updating-Your-Mods/Updating-Core-API-Changelog-1.4.0/). The adapter reads effective/permanent counts separately through public methods.

The actual own live subtotal, public buff identity dispatch, Harmony/Unity lifecycle, tooltip geometry, native cursor/input/pause, other mods and multiplayer behavior still require game testing. No exact multiplayer countdown is promised; remaining buff timers remain unavailable. Native UI architecture is unchanged until the first live test.

## Bounded robustness patch

The frozen 0.2.3 logic remains the mathematical reference. This patch improves unknown-state handling and display only. Omitted Knurl/Slug base-rate terms are now visible without Shift; unknown Slug activation does not imply an active bonus. Missing whole stat or shield observations remain unavailable. Missing master luck no longer fabricates a zero-luck probability. Protection pool hovers include current/max health and shield plus separate temporary barrier, without inferring a converted-health source total.

Public review on 2026-10-05 confirms that the [LookingGlass snapshot](https://github.com/Wet-Boys/LookingGlass/blob/4aa6dff73e9df72a50a67b9d9cf449bd6dcaf9cf/LookingGlass/ItemStats/ItemDefinitions.cs) describes Slug/Knurl base regen components using a difficulty-dependent helper and presents Transcendence as a capacity component. It does not establish the installed complete raw-regen placement or health/shield conversion order. The [R2API API documentation](https://github.com/risk-of-thunder/R2API/blob/master/R2API.RecalculateStats/RecalculateStatsAPI.cs) documents approximate independent health/shield/regen pools, not complete vanilla item ordering. Therefore this patch adds no exact math for either unresolved gap.

476 synthetic assertions pass; Unity/runtime validation remains unrun. Native Tab, input/cursor, pause, hover attachment and layout architecture are unchanged.
