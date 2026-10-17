# Exact source coverage: isolated 0.2.5 glossary expansion

Thirty-seven item IDs have partial quantitative component definitions. Nineteen buff names/states have explicit bounded glossary descriptions. This is source/build readiness, not runtime compatibility or complete source attribution.

n is the current effective total item count. Zero copies are explicitly inactive; an unknown count produces no numeric current/next prediction. A next copy means one extra EFFECTIVE copy with the stated conditions fixed, not a promise that a suppressed/disabled inventory becomes enabled. Permanent and effective counts stay distinct. b is an observed own buff stack/counter; u is the observed count of eligible unique active buff types.

## Item terms

| ID | Name | Registered current terms | Next-copy terms | Conditions / luck / missing effects |
|---|---|---|---|---|
| Syringe | Soldier's Syringe | 15n% attack-speed bonus | +15 pp bonus-pool term | Unaffected term; final attack-speed change differs with other sources |
| Hoof | Paul's Goat Hoof | 14n% movement bonus | +14 pp bonus-pool term | Unaffected term; sprint/survivor/other bonuses separate |
| AttackSpeedAndMoveSpeed | Mocha | 7.5n% attack-speed; 7n% movement bonus | +7.5 / +7 pp in separate pools | Unaffected shown terms; neither is a final-stat prediction |
| CritGlasses | Lens-Maker's Glasses | 10n raw crit percentage points | +10 raw points | Mixed: raw term unaffected, eligible critical roll affected; actual chance caps/converts |
| FlatHealth | Bison Steak | 25n flat health | +25 flat health term | Unaffected term; scaling/conversion separate |
| PersonalShield | Personal Shield Generator | 8n% max-health shield basis | +8 pp capacity term | Unaffected term; health basis/recharge/conversion not modeled |
| ArmorPlate | Repulsion Armor Plate | 5n flat incoming reduction | +5 reduction term | Unaffected; not armor points; minimum-damage floor/order can reduce actual benefit |
| FragileDamageBonus | Delicate Watch | 20n% unbroken damage bonus | +20 pp bonus term | Unaffected term; only unbroken effective count |
| Bear | Tougher Times | 100 x 0.15n / (1+0.15n)% block reference | Diminishing conditional chance difference | Public definition excludes luck adjustment; event blockability still contextual; not Safer Spaces |
| Missile | AtG Missile Mk.1 | 300n% triggering-hit missile coefficient | +300 pp coefficient | Mixed: eligible roll luck-sensitive; reference 10% at proc 1; extra copies do not change base roll |
| MissileVoid | Plasma Shrimp | 40n% triggering-hit missile coefficient | +40 pp coefficient | Shield-gated eligible nonzero-proc hits; deterministic trigger scope; shield-capacity effect unregistered |
| ChainLightning | Ukulele | 2n+1 maximum targets; 18+2n m chain reach | +2 targets; +2 m | Mixed: roll affected, range/count unaffected; 25% proc-1 base reference; secondary proc 0.2; damage unregistered |
| ChainLightningVoid | Polylute | 3n maximum strikes | +3 strikes | Mixed; same 25% proc-1 reference; extra copies do not change base chance; damage unregistered |
| BleedOnHit | Tri-Tip Dagger | min(100,10n)% reference chance at proc 1, luck 0 | Capped reference difference | Affected eligible roll; actual proc coefficient/luck unavailable; bleed damage/duration unregistered |
| BleedOnHitVoid | Needletick | min(100,10n)% reference chance at proc 1, luck 0 | Capped reference difference | Affected eligible roll; lower proc can still gain above ten copies; collapse damage/delay unregistered |
| Behemoth | Brilliant Behemoth | 4+2.5(n-1) m explosion radius | +2.5 m radius | Unaffected shown term; radius only, no per-target damage increase promised |
| ImmuneToDebuff | Ben's Raincoat | n prevention charges; fixed 10% max-health barrier/block; fixed 5 s exhausted recharge | +1 maximum charge; zero change in barrier percent/recharge | Ready/consumed state separate; no freeze/permanent/cooldown/Tonic Affliction immunity promise |
| SpeedBoostPickup | Elusive Antlers | 3n stack cap; 36n% full-stack movement maximum; interval 2+24/(n+2) s | +3 capacity; +36 pp full-stack maximum; shorter nominal interval | Unaffected terms; requires orb buffs; 12% per active stack; nominal buff 12 s; not passive held speed or old Antler Shield |
| EnergizedOnEquipmentUse | War Horn | 8+4(n-1) s nominal equipment-triggered buff | +4 s nominal duration | Unaffected term; attack-speed magnitude and remaining buff time unregistered |
| SpeedOnPickup | Collector's Compulsion | 2n+1 cap; 3(2n+1)% full-stack bonus component | +2 capacity; +6 pp full-stack component | Unaffected terms; 3% per active stack after pickups, nominal 20 s; exact individual stat application not fully attributed |
| ExtraStatsOnLevelUp | Prayer Beads | 20+5(n-1)% of a level-up stat-gain unit | +5 pp prospective storage rate | Unaffected term; counter b gives b x [0.20+0.05(n-1)] level-up equivalents, not current-stat percent; cached/partial removal omitted |
| BoostAllStats | Growth Nectar | 4n type capacity; active reference 4min(u,4n)% at known fixed u | 4[min(u,4n+4)-min(u,4n)] pp; often zero | Unaffected directly; unknown u shows labeled full-capacity maximum only; seven stat terms, acceleration excluded |
| ShockDamageAura | Faulty Conductor | 15 x 0.8^(n-1) s pulse interval; ideal min(1,7/interval) uptime | Exponential shorter interval; capped ideal uptime | Unaffected periodic/guaranteed-crit scope; ideal model assumes pulses reach/refresh subject; first continuous 7 s window at n=5; actual buff ID unverified |

| Knurl | Titanic Knurl | 40n flat health; 1.6n HP/s base regen reference | +40 health / +1.6 base HP/s | Raw regen placement unregistered; health term precedes shared scaling |
| Pearl | Pearl | 10n% health additive pool | +10 pp pool term | Not an independent final-health multiplier |
| ShinyPearl | Irradiant Pearl | 10n% each health/damage/regen pool; 10n raw crit points; early armor bonus factor 10n% | +10 in each respective component | Separate scopes/order; attack/movement components not registered here |
| SprintArmor | Rose Buckler | 30n conditional armor points | +30 conditional armor | Requires observed sprinting |
| OutOfCombatArmor | Oddly-shaped Opal | 100n conditional armor points | +100 conditional armor | Requires active ready buff; remaining cooldown unavailable |
| AttackSpeedOnCrit | Predatory Instincts | Fixed first-copy 5 raw crit; 36+24(n-1)% maximum triggered attack-speed component | Zero raw-crit delta; +24 pp maximum AS component | Crit rolls luck-sensitive; maximum requires all allowed buff stacks |
| HealOnCrit | Harvester's Scythe | Fixed first-copy 5 raw crit; 8+4(n-1) heal per eligible crit at proc 1 | Zero raw-crit delta; +4 reference healing | Heal event, not regen; actual attack proc/heal efficiency unavailable |
| BleedOnHitAndExplode | Shatterspleen | Fixed first-copy 5 raw crit; 400n% base-damage explosion coefficient; 15n% target-max-HP coefficient | Zero raw-crit delta; +400/+15 pp explosion components | Eligible bleed/kill required; no target-health read or full damage prediction |
| Infusion | Infusion | 100n stored-health capacity; n reference gain per eligible kill | +100 capacity / +1 reference gain | Current stat term uses observed stored bonus, not capacity; remaining cap limits gain |
| LunarDagger | Shaped Glass | 2^n independent damage factor; 100 x 0.5^n% health-basis reference | Double damage factor / halve remaining basis | Not predicted final HP; live model applies observed combined curse once |
| HealWhileSafe | Cautious Slug | 3n HP/s conditional base regen reference | +3 conditional base HP/s | Out of danger required; unregistered raw level/difficulty placement omitted |
| Crowbar | Crowbar | 75n% eligible high-health hit bonus | +75 pp hit component | Target state not read; excluded from CharacterBody.damage subtotal |
| BossDamageBonus | Armor-Piercing Rounds | 20n% eligible boss-hit bonus | +20 pp hit component | Enemy classification not read; excluded from raw damage |
| NearbyDamageBonus | Focus Crystal | 20n% eligible nearby-hit bonus | +20 pp hit component | Target distance not read; excluded from raw damage |

“Supported” means the listed component terms only. All other item IDs, consumed Watch/Sale Star states, other equipment and unlisted item effects retain native description/counts with formula/luck unavailable. A missing definition never means zero benefit. Sale Star remains a distinct reported-bug/unverified extra-item luck interaction with no adjusted chance.

The five live health/damage/regen/armor/crit rows use the original ordered contribution model in docs/CoreStatModel-0.2.4.md. Observed CharacterBody totals remain authoritative; the model never writes them. It combines reviewed baseline/growth, flat terms, shared additive pools, independent factors, conversions and late factors, then displays signed live-minus-modeled residuals. All attribution stays incomplete, including zero residuals. Regen item placement, Transcendence final health/shield conversion, other item/buff sources and hooks remain explicitly uncertain.

This build is isolated and unstaged. Installed 0.2.2 remains the first live-test candidate; reported 0.2.3 is also frozen. Native UI architecture is unchanged; only the stat/item data explanations have expanded.

The attack-roll estimator is an isolated conditional reference requiring an explicit proc coefficient and luck. The runtime tooltip adapter never supplies an invented current-attack coefficient; displayed Dagger/Needletick chances are visibly proc-1, zero-luck REFERENCES. AtG/Ukulele/Polylute give contextual base references only. Proc-chain masks, target eligibility, item conversions and mod balance remain unmodeled.

## Buff glossary

The following 19 named states have original reference descriptions, count meanings, conditions, pinned source/version confidence and unavailable timers. Public identities are matched through observed public buff objects in the data adapter; a similar internal name alone does not select an effect.

| Public identity | Display name | Count/state meaning |
|---|---|---|
| DLC1Content.Buffs.BearVoidCooldown | Safer Spaces cooldown | Cooldown state |
| DLC1Content.Buffs.BearVoidReady | Safer Spaces ready | Ready state |
| DLC1Content.Buffs.ImmuneToDebuffCooldown | Ben's Raincoat recharge state | Recharge state |
| DLC1Content.Buffs.ImmuneToDebuffReady | Ben's Raincoat ready charges | Ready charges |
| DLC1Content.Buffs.OutOfCombatArmorBuff | Oddly-shaped Opal ready armor | Ready state |
| DLC2Content.Buffs.BoostAllStatsBuff | Growth Nectar counter | Type counter |
| DLC2Content.Buffs.ElusiveAntlersBuff | Elusive Antlers speed | Active stat stacks |
| DLC2Content.Buffs.ExtraStatsOnLevelUpBuff | Prayer Beads stored level gains | Stored level counter |
| DLC2Content.Buffs.KnockBackAvailable | Breaching Fin ready | Ready state |
| DLC2Content.Buffs.KnockBackUnavailable | Breaching Fin cooldown | Cooldown state |
| DLC3Content.Buffs.JumpDamageStrikeCharge | Faraday charge | Charge counter |
| DLC3Content.Buffs.SpeedOnPickup | Collector's Compulsion | Active stat stacks |
| RoR2Content.Buffs.AttackSpeedOnCrit | Predatory Instincts attack speed | Active stat stacks |
| RoR2Content.Buffs.Energized | War Horn energized | Active state |
| RoR2Content.Buffs.FullCrit | Ocular HUD critical buff | Active state |
| RoR2Content.Buffs.MedkitHeal | Medkit pending heal | Pending heal |
| RoR2Content.Buffs.TeslaField | Unstable Tesla Coil active | Active state |
| RoR2Content.Buffs.Warbanner | Warbanner | Active state |
| RoR2Content.Buffs.WhipBoost | Red Whip active | Active state |

Detailed effect references, conditions, legacy API aliases, exact source links and confidence are in [the source inventory](../artifacts/buff-source-inventory.json). All descriptions remain public-snapshot references; compilation does not establish Unity dispatch or installed behavior.

Nectar assessment is separate from the effect description. Observed active count and live hidden/ignoreGrowthNectar/catalog metadata plus observed self-counter identity produce **eligible type**, **not counted**, or **unknown**. Charges, cooldown/debuff labels, icon/color and stack size do not invent eligibility. Each qualifying active type counts once. Missing catalog context yields unknown unless a known exclusion already settles the result. The game's own Nectar count remains authoritative.

Zero/negative observed counts do not produce an active glossary row. Hidden or unresolved visibility is filtered. Unfamiliar visible definitions show an internal ID, observed metadata and an explicit unknown-effect description. HiddenInvulnerability and the broad HiddenInvincibility name remain unregistered; hidden flags are honored, and no immunity is guessed from a name.

Remaining timers are unavailable for every state. Medkit's nominal delay and Fin's nominal cooldown are references, not live countdowns. No private timer reflection, networked timer dependency or exact multiplayer countdown is introduced.

## Sources and confidence

- Mathematical component facts and public IDs: LookingGlass LGPL-3.0 source snapshot 4aa6dff73e9df72a50a67b9d9cf449bd6dcaf9cf:
  https://github.com/Wet-Boys/LookingGlass/blob/4aa6dff73e9df72a50a67b9d9cf449bd6dcaf9cf/LookingGlass/ItemStats/ItemDefinitions.cs
- Public ID/name catalog:
  https://risk-of-thunder.github.io/R2Wiki/Mod-Creation/Developer-Reference/Items-and-Equipments-Data/
- Effective inventory API context:
  https://risk-of-thunder.github.io/R2Wiki/Mod-Creation/Updating-Your-Mods/Updating-Core-API-Changelog-1.4.0/
- Metadata eligibility reference, MIT GrowthNectarDisplay public project/source:
  https://github.com/Goorakh/RoR2_GrowthNectarDisplay
  https://old.thunderstore.io/package/Goorakh/GrowthNectarDisplay/source/
- Supplementary public gameplay/status details supplied through the parent's independent source review on 2026-10-05: Prayer Beads, Nectar, Antlers, Compulsion, Raincoat and Conductor wiki pages. The direct wiki fetches failed here; installed-build behavior was not independently tested:
  https://riskofrain2.wiki.gg/wiki/Prayer_Beads
  https://riskofrain2.wiki.gg/wiki/Growth_Nectar
  https://riskofrain2.wiki.gg/wiki/Elusive_Antlers
  https://riskofrain2.wiki.gg/wiki/Collector%27s_Compulsion
  https://riskofrain2.wiki.gg/wiki/Ben%27s_Raincoat
  https://riskofrain2.wiki.gg/wiki/Faulty_Conductor

All registry predictions are labeled conditional public-snapshot components. Source/revision and supplementary context are in full item details. Earlier FullCrit evidence is recorded in the existing project audit; no new game implementation inspection occurred. Original independent expressions/presentation were authored here; no LookingGlass implementation, proprietary source/assets or external runtime dependency were copied into the plugin. Public source review is not a legal approval or audit of installed game behavior.

Sale Star report source: https://riskofrain2.wiki.gg/wiki/57_Leaf_Clover . Its reported extra-item luck issue stays unverified and distinct from unsupported or confirmed-unaffected.

## Validation

525 synthetic assertions pass. Fixed expected current and next values at n=1/4/10 cover every registered item term, including nonlinear/capped curves, fixed Raincoat components, Antlers interval and Conductor's n=4/5 uptime threshold. Separate checks cover low-proc chance beyond ten Daggers, luck order, unknown contexts/effects, Nectar zero/immediate deltas, stored Bead units, numeric underflow and metadata exclusions.

These are synthetic mathematical/presentation tests. They do not exercise Unity, Harmony installation, real item events, scene lifecycle, aliases, actual pause/input or other mods.

Five-stat reconciliation also tests shared pools versus compounding, conversion order, positive/negative residuals, separate protection pools, effective/temporary/disabled/unknown counts, conditional/inactive buffs, numeric overflow, and a small residual retained inside comparison tolerance.

The inherited 0.2.4 stat patch adds no exact conversion/scaling formula or new item definition. Short regen hovers now name active omitted base-rate terms or unknown Slug activation. Protection hovers show current/max HP, current/max shield and barrier separately; missing shield inputs remain unavailable. Missing whole stat observations and missing luck sources retain explicit unavailable labels.

The 0.2.5 pass expands only glossary/metadata presentation. Installed 0.2.2 and reported 0.2.3/0.2.4 remain frozen; the user should test stable 0.2.2 first. No automatic upgrade occurs.
