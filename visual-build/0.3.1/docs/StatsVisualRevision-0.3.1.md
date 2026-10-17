# Stats visual revision 0.3.1

Isolated source-only successor to installed 0.3.0. No deployment or game interaction. The parent inspected all three images and supplied verified pixel descriptions; the blocked Windows Library transfer was not repeated.

## Requested presentation

Centered, underlined **Stats** title; full-width category bands with large opaque white icons, thin white horizontal rules and right-aligned **Expand / Collapse** text. Uses the existing native RoR2 TMP font and material.

Scroll categories: Defense; Offense (crossed swords); Movement (running person); On-Hit Effects; On-Kill Effects; Out of Combat; While Sprinting; Misc Effects. Defense starts expanded; the others start collapsed. No visible Base Stats wrapper or collapsible Luck category.

General Damage Modifiers has no separate section. Its items remain under Misc Effects; conditional-hit notes appear in expanded item Details. No functional rows were dropped. Out of Combat and While Sprinting remain standalone as the user clarified.

The green Luck footer sits outside the masked scroll content, with an opaque clover, label and right-aligned signed live value. Body and footer backgrounds each have 80% alpha and occupy separate regions. Text/icons are opaque. Native tooltip backgrounds and menu blur remain unchanged.

Preferred width is 425 at scale 1. Existing safe placement may reduce width or decline the sheet when space is insufficient. Height is bounded; added sections and expanded inventory can require scrolling. This follows the Figma structure using native typography; pixel-identical rendering is not claimed.

## Stats and effects

Right-aligned Defense totals retain Health, conditional Shield capacity, Regen, raw Armor and Dmg Reduction. Depleted shield retains its row while maximum capacity exists. Armor-derived percent stays on hover. Flat Dmg Reduction is the maintained Armor Plate component with per-hit conditions/floor, not complete mitigation.

Offense retains Damage, Attack Speed, general Cooldown Reduction and Crit Chance. General CDR stays Unknown; shared-item references remain separate in Details and skill-specific modifiers are excluded.

Movement retains Movement Speed, Sprint Speed Modifier and Maximum jumps (capacity, not remaining). Luck is a signed live stat, not a probability. Barrier remains separate from shield. Existing buff glossary and passive/item rows remain.

## Item Details order

The native title and description stay intact through both scoreboard and top-HUD inventory paths. Added calculations use owned UI below the native tooltip; existing placement decline/pinning and pointer-transfer leases remain.

Above one effective copy, numeric Single-copy components precede Current stack (N effective). At one copy the native description already gives the individual effect, so redundant added individual/current-stack blocks are omitted. Next-copy benefits, conditions and owned/effective distinction remain. Unknown formulas are not guessed.

Conditional damage notes appear in expanded Details. Target-health, proximity, watch break-state and other trigger/target requirements remain separate from live Damage. No enemy state is inspected.

## Display residual precision

The screenshot's approximately 4.47e-8 /s Regen difference is float roundoff. Only the compact Other/unknown residual is filtered using the larger of 1e-12 absolute tolerance and 6e-8 times max(abs(live), abs(modeled)).

Authoritative values, known small bonuses, ordered calculation terms and exact Details traces are unchanged. Real residuals above tolerance remain. Near-zero values are preserved rather than broadly rounding all small values away. At large magnitudes a display comparison cannot distinguish changes below float precision; this does not certify complete attribution.

## Growth Nectar

The appended compact note refreshes through the existing 0.2-second live-data cycle while hovering. Same-item updates preserve open Details.

Shows Effective buffs: used / observed cap; eligible types when over cap or unknown; known reference bonus, inactive 0%, or Unknown; and a counter/model discrepancy label where needed.

Observed count and cap come from public CharacterBody members. Displayed used count is bounded by observed cap. The separate metadata model counts distinct active types, not stacks/all HUD icons; excludes hidden definitions, definition/catalog ignoreGrowthNectar flags and the public Nectar counter definition itself. No guessed name matching. Missing active metadata or contradictory duplicate observations makes eligibility unknown.

The current [LookingGlass public formula](https://github.com/Wet-Boys/LookingGlass/blob/master/LookingGlass/ItemStats/ItemDefinitions.cs) was reviewed again: capacity is four types per effective copy and the reference component is 4% per used type. No implementation was copied.

Bonus is a reference Nectar component when its public counter buff is observed active and observed cap matches per-copy reference capacity. An absent counter buff shows inactive 0%; missing activation/formula or unexpected cap gives Unknown. Full final stat attribution is not claimed. Existing next-copy model remains conditional at a fixed eligible-type set and predicts no activation transition.

Details separates eligibility, raw game counter, observed/reference cap and activation. Buff glossary shares null-safe eligibility and capped/active presentation. Unknown catalog metadata does not trigger a null-array failure.

## Focus and lifecycle

Native HGButton/MPEventSystem route remains. Up/Down navigates visible content then fixed Luck. Left/Right collapses/expands headers; stat Right opens Details and stat Left returns to its category where available. Submit toggles the focused header/Details.

Footer focus does not scroll content. Collapse restores focus to a visible category; shorter content clamps scroll offset. Existing ownership checks, native item focus cleanup, pause/session guards and disposal remain. Plugin changes are only version strings and reference defaults for the existing memory-only collapse state. No Shift/raw keyboard polling or gameplay changes.

## Verification and remaining live checks

15,142 assertions passed: inherited 9,928 plus 5,214 added checks. Includes 256 flat collapse states, 108 footer/scroll sizing cases, shield/focus projection, float precision, Nectar distinct types/exclusions/cap/activation/unknowns/discrepancies and item order. Final plugin compiled without warnings against public reference assemblies; it was not executed.

Still check native appearance, opacity, clipping and icons; long-content/footer separation; keyboard/controller focus entry and cleanup; both inventory tooltip routes, one/multiple-copy Details and pointer transfer; Nectar gaining/losing types, changing capacity, activation/self exclusion and any discrepancy; Regen meaningful small bonuses; repeated Tab/menu/pause cleanup and multiplayer click suppression where available.

General CDR remains Unknown; complete stat attribution remains partial. Quantitative item coverage remains 23 definitions despite 44 routing definitions. The user's 0.3.0 report does not validate 0.3.1. Deployment needs separate approval and a fresh game-closed check.

