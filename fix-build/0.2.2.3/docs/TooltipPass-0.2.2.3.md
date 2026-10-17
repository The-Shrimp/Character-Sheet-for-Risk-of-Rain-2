# Tooltip pass 0.2.2.3 — source review candidate

This candidate branches from 0.2.2.2. It is not installed. This task did not launch, close, control or interrupt the user's game. No software installation, proprietary implementation inspection, profile write or publishing occurred in this pass.

## Reviewable behavior

Default item hover contains the native pickup sentence, current effective-stack component results, a short first-copy/additional-copy hint for additive terms, and one concise reference label. Unknown scaling says “Scaling unverified”. Permanent/effective counts appear separately when they differ. Item count remains in the title. Nonlinear equations, explicit next-copy amounts and marginal benefit, conditions, sources and the native full description are behind Shift.

Default base-stat hover contains total, survivor base, nonzero level growth, applicable known flat terms by source, applicable known modifiers by source, and a concise unattributed remainder. No unavailable percentage modifiers are invented. A zero remainder adds no warning or repeated subtotal to the default view; Shift retains model coverage limits. Shield/barrier stay separate. Inherited attribution coverage remains primarily survivor base, level growth and the explicit Glasses crit term. A synthetic health example is:

    Total health: 150
    Base health: 100
    Level growth: +20
    Unattributed: +30 (partial)

Expanded stat math labels each growth/flat operand as its contribution and retains the original calculation-stage order. Compact category grouping does not recalculate or reorder the model. Only applicable nonzero flat/growth rows are shown. Live totals remain authoritative.

Buff glossary rows use a short effect, type/counter distinction, Nectar eligibility and “Timer unavailable”. Existing eight glossary definitions and detailed evidence remain behind Shift. No timer adapter or new effect formulas were added.

The owned popup measures content height and limits long bodies to a scrolling viewport; its title is not duplicated in its body. The footer is one short Shift hint plus a small clover badge; hovering the badge shows its scoped luck explanation. Resize clamps the existing popup position even while the pointer remains inside it.

## Native formatting

At runtime, the adapter observes existing native TooltipController public titleLabel/bodyLabel fields and public Unity image components. It borrows title/body font, material, normal color, font formatting, spacing, style-sheet and available image references into owned UI. It never instantiates or modifies a native tooltip or shared asset. No new semantic RGB palette was chosen. Background selection now requires body coverage and prefers observed native fill components over border/outline/shadow/glow decorations. The chosen native sprite, material, tint and opacity are copied exactly; runtime readability still requires review.

Native pickup/full-description formatting passes through a restricted renderer. Actual native named styles are resolved through the observed body style sheet or TMP_Settings.defaultStyleSheet. Native explicit hex colors are retained exactly, including a fire color when native text supplies one. Unknown styles degrade to normal text. Font/body/image availability and exact visual correspondence remain runtime checks; this is not a claim of pixel matching.

Own calculated terms request native damage/healing/utility/stack style names. Existing native descriptions determine their own fire/status formatting; unsupported buffs receive no fabricated effect/color. Links, sprites, arbitrary font/material/size tags and untrusted markup are excluded. Plain own strings are escaped, supported tags balanced, and whitespace preserved.

Public references: [TMP text properties](https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.0/api/TMPro.TMP_Text.html), [native style lookup](https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.0/api/TMPro.TMP_StyleSheet.html), and [LookingGlass native-description/style usage](https://raw.githubusercontent.com/Wet-Boys/LookingGlass/master/LookingGlass/ItemStats/ItemStats.cs). No LookingGlass implementation was copied. Inherited definitions and provenance notes remain unchanged.

## Hook and calculation coverage

| Category | Candidate coverage | Remaining gap |
| --- | --- | --- |
| Own native scoreboard item icons | Existing exclusive own-player ItemIcon / TooltipProvider routing preserved. Diagnostics report ready targets separately from registered formulas. | Actual pointer delivery, provider conflicts and recycled/disabled targets need runtime observation. |
| Added character stat rows | Existing rows connected. | Complete item/buff/conversion attribution unavailable; an attribution gap does not prove a missed pointer hook. |
| Item formulas | 23 registered definitions; every owned visible item still gets native text. | Other definitions remain quantitatively unverified. |
| Attack calculations | Component estimates retain assumptions; no invented hit data. | Actual attack proc coefficient, eligibility and proc-chain context unavailable. Proc-1/luck-0 references stay labeled. |
| Added buff glossary rows | Existing expanded Effects & buffs rows connected; eight inherited definitions. | Many effects and per-stack remaining timers unavailable. |
| Native HUD buff icons | No new icon hook. | Separate, untested 0.2.6.1 adapter. |
| Expanded native inventory viewport | Native inventory unchanged. | Separate 0.2.6.1 scrolling/resizing adapter. |
| Native equipment / skill icons | Existing native behavior retained. | Custom adapters outside this candidate. |

Registered IDs: ArmorPlate, AttackSpeedAndMoveSpeed, Bear, Behemoth, BleedOnHit, BleedOnHitVoid, BoostAllStats, ChainLightning, ChainLightningVoid, CritGlasses, EnergizedOnEquipmentUse, ExtraStatsOnLevelUp, FlatHealth, FragileDamageBonus, Hoof, ImmuneToDebuff, Missile, MissileVoid, PersonalShield, ShockDamageAura, SpeedBoostPickup, SpeedOnPickup, Syringe.

No 0.2.6.1 source was imported. Both original 0.2.6 and 0.2.6.1 DLL hashes still match the saved checkpoint.

The ordinary Dev log last written 2026-10-06T02:35:23.3733173Z records 0.2.2.2 and six compact attachment messages, with no own-plugin warning/error among selected messages. It does not establish missing-hook identity, distinct Tab timing, pause/resume correctness or current vanilla/modded launch state.

Parent pixel review covers screenshot IDs libfile_a6aa21741e648191968670d107d3c5bb and libfile_82c354bbb3e48191b7b44efc126bc0c0. It confirms verbose plain Tri-Tip1 text and a level4 health209/base110 case that mislabeled cumulative209 as growth. The corrected default shows exactly total209, base110 and growth+99. Shift also labels +99 as the contribution. No broken/disconnected hover target is visible in either screenshot; “missing hooks” may refer to earlier model boilerplate and is not diagnosed as a pointer-hook failure. Parent review is used as screenshot evidence; no additional Library attempt made here.

## Validation and preservation

- 2,757 assertions pass, including 400 added assertions and 96 tooltip sizing cases.
- All 23 definitions exercised at 0/1/4/10 effective copies.
- Tests cover additional-copy math, unavailable formulas/hit context, capped-roll assumptions, multiple components, conditional references, markup safety/whitespace, partial attribution, applicable source groups and preserved calculation order.
- Original tests retained; lifecycle/ownership/pause regressions pass.
- Plugin compiles against local read-only assembly references; no implementation inspection performed.
- Protected source and docking geometry checked against 0.2.2.2. Plugin.cs differs only in two version strings.
- Final DLL: 131,072 bytes. SHA256: 49F6A0B90ADF8D341CD457CCB0C7719B9273ABE904D8A22B996B726E03A6D915.
- Actual host installed 0.2.2.2 DLL SHA256: 6C54A2FACA7A9751DD240EF3514647347C4B2BD86660378177C11F662964A396.
- Eight of nine supporting files match the 02:20 deployment receipt. The own configuration differs only in its generated comment header, v0.2.2.1 to v0.2.2.2. Reversing that one header change reproduces the earlier SHA256 exactly, proving zero section/key/value delta. Created 2026-10-05 23:39:27.672227 UTC; last written 2026-10-06 02:27:20.307839 UTC; observed 02:50:57.943173 UTC. Cause is not inferred; no edit was issued. See artifacts/CONFIG-DELTA.json. Core loader, Harmony, Json/license and other recorded files remain matched.
- Candidate created at 2026-10-06T02:30:43.615076+00:00. No deployment.

Evidence: artifacts/TOOLTIP-VERIFICATION.json, artifacts/UNCHANGED-INSTALLATION-CHECK.json artifacts/TOOLTIP-PASS.diff and artifacts/CONFIG-DELTA.json.

## Tomorrow's review

No live testing requested tonight. Review this separate candidate and obtain explicit deployment authorization before installation. Keep installed 0.2.2.2 and previous backups.

After authorized installation, inspect a health/base-stat hover, multi-term item and unsupported item; compare default versus Shift. Verify native hierarchy/semantic formatting, fire/status description formatting where present, readable fonts, edge/scroll bounds and clover detail. Identify the specific missing item/buff/target and use ready-target-versus-formula diagnostics. Continue the outstanding hold/release and pause/resume checks. Runtime results remain unverified.

Final corrected candidate SHA256: 49F6A0B90ADF8D341CD457CCB0C7719B9273ABE904D8A22B996B726E03A6D915. This supersedes 44D382092AB89DE9BF33F9FD568742F46E1598944A8CAFC698AD6FEEF9A42EB0.
