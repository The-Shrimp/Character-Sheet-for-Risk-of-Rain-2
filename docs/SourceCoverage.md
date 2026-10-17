# Exact source coverage: 0.2.2

Twenty-three item IDs have partial quantitative component definitions. Eight buff names/states have explicit glossary descriptions. This is source/build readiness, not runtime compatibility or complete source attribution.

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

“Supported” means the listed component terms only. All other item IDs, consumed Watch/Sale Star states, other equipment and unlisted item effects retain native description/counts with formula/luck unavailable. A missing definition never means zero benefit. Sale Star remains a distinct reported-bug/unverified extra-item luck interaction with no adjusted chance.

The actual live stat rows still use authoritative CharacterBody totals. Their source explanation covers survivor baseline, level growth and the earlier crit item term only. This coverage pass does not make final health/damage/regen/armor/crit attribution complete. Extra component definitions have not been injected into that calculation order.

The attack-roll estimator is an isolated conditional reference requiring an explicit proc coefficient and luck. The runtime tooltip adapter never supplies an invented current-attack coefficient; displayed Dagger/Needletick chances are visibly proc-1, zero-luck REFERENCES. AtG/Ukulele/Polylute give contextual base references only. Proc-chain masks, target eligibility, item conversions and mod balance remain unmodeled.

## Buff glossary

| Matched public identity/name | Public name and bounded effect |
|---|---|
| RoR2Content.Buffs.FullCrit | Ocular HUD crit buff; earlier checked +100 raw crit points before conversions; not certified final damage |
| DLC2Content.Buffs.BoostAllStatsBuff | Growth Nectar qualifying-type counter; 4% reference per counted type; authoritative own current/max counter preserved |
| DLC2Content.Buffs.ExtraStatsOnLevelUpBuff | Prayer Beads stored level-gain counter; b times current-copy reference rate, not percent-current-stat or certified cached payout |
| ElusiveAntlersSpeedBuff | Elusive Antlers speed: 12% movement component per observed active stack; nominal 12 s |
| SpeedOnPickup | Collector's Compulsion: 3% bonus component per observed active stack; nominal 20 s; individual stat application incomplete |
| ImmuneToDebuff | Raincoat ready-charge state; bounded temporary-debuff prevention, documented barrier component; metadata eligibility read separately |
| ImmuneToDebuffConsumed | Raincoat consumed/recharge state; nominal 5 s exhausted recharge, not an always-active defense multiplier |
| Energized | War Horn equipment-triggered attack-speed buff; item duration component registered, magnitude unregistered |

Known names use an original registry; documented short names accept an optional leading bd. Actual name matching/UI dispatch has not been runtime-tested. All other own active visible buffs retain definition-derived names, observed count, canStack/isDebuff/isCooldown metadata and an explicit unverified-effect label. Conductor's actual buff identity is unverified and is not guessed into this registry.

Remaining duration is UNAVAILABLE for every buff. Nominal source lifetimes are not per-instance or per-stack countdowns. No private timer reflection or multiplayer timer guarantee is used.

Nectar eligibility uses own live public metadata: exclude hidden definitions, ignoreGrowthNectar, catalog exclusions and its own counter. Count each active eligible type once, independently of stack count or guessed debuff/icon meaning. The modeled current/fixed-u-next term is shown beside the game's authoritative counter, which remains the source of truth. Missing catalog/definition context gives unknown, rather than invented actual bonus. Bead stored counters and ready/recharge states use the same metadata rule.

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

Fixed expected current and next values at n=1/4/10 cover every registered item term, including nonlinear/capped curves, fixed Raincoat components, Antlers interval and Conductor's n=4/5 uptime threshold. Separate checks cover low-proc chance beyond ten Daggers, luck order, unknown contexts/effects, Nectar zero/immediate deltas, stored Bead units, numeric underflow and metadata exclusions.

These are synthetic mathematical/presentation tests. They do not exercise Unity, Harmony installation, real item events, scene lifecycle, aliases, actual pause/input or other mods.
