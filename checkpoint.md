# Risk of Rain 2 Character Sheet checkpoint

Updated: 2026-10-07 UTC.

## Status and scope

This project is paused while the user's separate Game Night bot is restored. The latest installed release is **0.3.1**. The user now says the latest version "looks good"; this is positive appearance feedback, not specific validation of Nectar transitions, pause restoration, controller focus or multiplayer behavior.

This checkpoint was created after read-only verification. No game launch, profile/configuration change, rebuild, feature work or deployment occurred while writing it. Continue only when the user returns to this project.

Project root:

~~~text
C:\Users\heale\Desktop\Not School Stuff\Risk of Rain 2 Stat Sheet\Character Sheet for Risk of Rain 2
~~~

The mod is a presentation-only addition to the native own-character Tab/Info sheet. No gameplay/stat changes, hidden loot/enemy information or automation. Preserve unrelated projects and user files.

## Installed release and actual host path

Installed 0.3.1 on 2026-10-06 at approximately 21:57 UTC, after user approval and a fresh game-closed check. Only the custom DLL was replaced. Exactly one custom DLL and all nine checked supporting files were verified at installation.

Rechecked on 2026-10-07: installed and reviewed DLLs match, each **181,248 bytes**:

~~~text
SHA256: 9B3CD10CCEA8823E3ABAFE11CA1F6025737B1D4B60C711CDD8FB7C315A8DBB25
~~~

Actual host DLL:

~~~text
C:\Users\heale\AppData\Roaming\r2modmanPlus-local\RiskOfRain2\profiles\AlexanderCharacterSheet-Dev\BepInEx\plugins\Alexander-CharacterSheet\Alexander.CharacterSheet.dll
~~~

Reliable host path from the connected Codex environment:

~~~text
\\localhost\C$\Users\heale\AppData\Roaming\r2modmanPlus-local\RiskOfRain2\profiles\AlexanderCharacterSheet-Dev\BepInEx\plugins\Alexander-CharacterSheet\Alexander.CharacterSheet.dll
~~~

Codex's packaged process can redirect ordinary AppData/Roaming accesses to a package shadow. Do not assume a successful ordinary-path write reaches the game's profile. Use the existing localhost UNC path for host verification and independently check the exact DOS path with Windows CIM file metadata. Do not create/change shares or credentials. Fresh UNC hashing and host CIM both confirmed 181,248 bytes; one custom DLL exists in its directory and one matching named DLL across plugins.

The eight other supporting-file hashes still match deployment. The plugin's own config has only its generated version comment changed from v0.3.0 to v0.3.1 by its subsequent startup; normalizing that comment reproduces the deployment hash. Settings values match. Do not rewrite the config to remove this expected comment change.

## Working source and lineage

**Resume from visual-build\0.3.1**, not the older root src directory.

Reviewed source/binary:

~~~text
visual-build\0.3.1\src
visual-build\0.3.1\tests
visual-build\0.3.1\artifacts\review-native-tab\Alexander.CharacterSheet.dll
~~~

Lineage:

- fix-build\0.2.2.5: native item-tooltip preservation, simple stat hovers and clickable owned Details.
- hierarchy-build\0.3.0: grouped/collapsible hierarchy, corrected Defense semantics, 80% sheet background and effect routing.
- visual-build\0.3.1: user's Figma structure and clarified category order, fixed Luck footer, individual/stack item presentation, residual precision and live Nectar counter.
- next-build\0.2.6 and next-build\0.2.6.1: separate, undeployed advanced inventory/buff work. They were not wholesale merged. Native buff-icon hover, larger inventory presentation and expanded buff coverage must be reviewed individually before importing any part. Do not replace the current DLL with these older separate candidates.

The authoritative live stat adapters, ordered math, native tooltip anchor/style, two inventory ownership paths and pause/session cleanup were preserved through the visual revision.

Useful current source entry points:

- src\Presentation\SheetView.cs: panel, category bands, fixed footer, navigation and safe attachment.
- src\Core\VisualSheetStructure.cs: visual category projection, collapse labels and footer/viewport geometry.
- src\Presentation\SheetStructureData.cs: live rows and conditional effect notes.
- src\Core\IndividualItemDetails.cs: single-copy-before-stack display.
- src\Core\DisplayResidual.cs: compact residual tolerance.
- src\Presentation\NectarObservationAdapter.cs and src\Core\NectarCounterPresentation.cs: public live observations, eligibility, cap/bonus and unknown handling.
- src\Plugin.cs: existing native session, pause and ownership guards.

All entry points above are relative to visual-build\0.3.1. Make a new isolated successor before changing the reviewed release.

## User-approved visual structure

Fixed centered, underlined **Stats** title. Slim single column, full-width category header bands, large opaque white icons, thin white horizontal rules, and right-aligned explicit **Expand / Collapse** text. Native RoR2 TMP font/material is used; no arbitrary replacement font or extracted asset.

Scroll sections, in order:

1. Defense
2. Offense, crossed swords
3. Movement, running person
4. On-Hit Effects
5. On-Kill Effects
6. Out of Combat
7. While Sprinting
8. Misc Effects

Defense starts expanded; other sections start collapsed. There is no visible Base Stats outer accordion. General Damage Modifiers has no standalone header: its item rows remain under Misc Effects, and conditional damage information appears in individual item Details.

A bright green fixed **Luck** footer sits outside the scroll content, with a clover, label and right-aligned signed live value. Luck is a raw stat, not a probability. Content is masked above the footer and cannot scroll behind it. Footer focus does not scroll the content.

Body and footer backgrounds each use 80% alpha in separate, nonoverlapping regions. Text/icons remain opaque; no CanvasGroup opacity affects them. Native tooltip backgrounds and native menu blur are preserved. Preferred width at scale 1 is 425; safe native placement can reduce it or decline the added panel when space is insufficient.

Right-aligned stat totals remain:

- Defense: Health; Shield only while actual maximum shield capacity exists; Regen; raw Armor; Dmg Reduction.
- Armor-derived reduction percent is on hover. Flat Dmg Reduction is the known Armor Plate per-hit component with conditions/floor; it is not armor percentage or complete mitigation.
- Offense: Damage, Attack Speed, general Cooldown Reduction, Crit Chance.
- Movement: Movement Speed, Sprint Speed Modifier, Maximum jumps (maximum capacity, not remaining jumps).
- Barrier stays separate from shield. Depleted shield retains its capacity row while maximum capacity is positive.

Passive/item On-Hit effects exclude active ability ignite. On-Kill includes item effects such as Gasoline/Wisp. Slug's out-of-danger condition is distinct from out of combat; Quail requires sprint plus jump, distinct from merely sprinting.

## Hovers, item extensions and Nectar

Ordinary stat hovers use **Total / Base / Bonuses**. Base shows survivor baseline and actual per-level coefficient; level growth is applied once. Known bonuses are readable flat/percent entries; meaningful remaining differences are Other/unknown. Optional exact Details retains operation order and partial-attribution context.

The Regen approximately 4.47e-8 /s residual was float roundoff. Only compact Other/unknown display residuals use max(1e-12 absolute, 6e-8 relative to max(abs(live), abs(modeled))). Authoritative values, known small bonuses and exact traces remain untouched.

The actual native item title/description remains intact through both own-scoreboard and top-HUD inventory routes. Owned additions appear below it, with safe-room decline, source identity and pointer-transfer cleanup preserved. **Details is clicked; no Shift shortcut or raw keyboard polling.** Clover applicability information remains scoped to eligible rolls.

Individual effects come first. When effective quantity exceeds one, added **Single-copy components** precede **Current stack (N effective)**. At one copy the native description supplies the individual effect, so redundant added individual/current-stack blocks are omitted. Next-copy benefits, conditions and owned/effective distinction remain. Unknown formulas are not invented.

Growth Nectar's compact appended tooltip refreshes on the existing 0.2-second live-data cycle, including while continuously hovered, without resetting open Details. It shows Effective buffs: used / observed cap; distinct eligible types when over cap or unknown; active reference bonus, inactive 0%, or Unknown; and any model/game-counter discrepancy.

Eligibility counts distinct active buff types, not stacks or every HUD icon. It excludes hidden definitions, definition/catalog ignoreGrowthNectar flags and the public Nectar counter buff itself. Missing metadata or contradictory observations is unknown; no name-based guessing.

Public reference: four types of capacity per effective copy, 4% per used type. The compact bonus is a reference component when the observed counter buff is active and observed cap matches reference capacity. Unexpected cap/missing activation/formula gives Unknown; the game observations remain authoritative. This is not complete final stat attribution. Next-copy estimates hold the same eligible type set and do not predict activation transitions.

## Loader and build setup

- Existing per-user r2modman, isolated AlexanderCharacterSheet-Dev profile.
- Core-only BepInExPack 5.4.2122 setup plus reviewed loader dependencies. Do not silently install the complete extra dependency bundle.
- LookingGlass is a public reference, not a required runtime dependency; no implementation was copied. No further proprietary game implementation/IL inspection.
- Signed Newtonsoft.Json **13.0.4** is present in the actual profile BepInEx\core directory. File version 13.0.4.30916; assembly identity Newtonsoft.Json, Version=13.0.0.0, Culture=neutral, PublicKeyToken=30ad4fe6b2a6aeed. Assembly version 13.0.0.0 is expected and is not evidence of an older file. Do not substitute an unsigned build or alter the game's JSON assembly.
- Normal bootstrap is disabled: enabled=false freshly verified in both profile and Steam game doorstop_config.ini. Normal Steam launch was previously confirmed to run without mods. Use the manager's modded launch for Dev-profile tests when the user chooses to test; do not enable the default bootstrap.
- No software, SDK, sleep/security setting or Steam preference changes are needed to resume source work.

Compiler already available:

~~~text
C:\Program Files (x86)\Microsoft Visual Studio\18\BuildTools\MSBuild\Current\Bin\Roslyn\csc.exe
~~~

Public game assembly reference directory:

~~~text
C:\Program Files (x86)\Steam\steamapps\common\Risk of Rain 2\Risk of Rain 2_Data\Managed
~~~

Build-only reviewed loader references:

~~~text
<repository root>\.references\BepInExPack-5.4.2122
~~~

Build/test commands for the current source (do not execute just to read this checkpoint):

~~~powershell
$rorProject = 'C:\Users\heale\Desktop\Not School Stuff\Risk of Rain 2 Stat Sheet\Character Sheet for Risk of Rain 2'
Push-Location -LiteralPath (Join-Path $rorProject 'visual-build\0.3.1')
try {
    & '.\build.ps1' -Mode CoreTests
    & '.\build.ps1' -Mode PluginReview -LoaderCorePath (Join-Path $rorProject '.references\BepInExPack-5.4.2122')
} finally {
    Pop-Location
}
~~~

Evidence for reviewed 0.3.1: final plugin compiled without warnings; **15,142 assertions passed**, including inherited 9,928 plus 5,214 new assertions. Added checks cover 256 collapse combinations, 108 footer/scroll layouts, residual precision, distinct Nectar types/exclusions/cap/activation/unknowns/discrepancies and individual-before-stack order. Inherited checks include native pointer transfer, 24 sessions x 16 items, 96 tooltip sizing and 36 addon placement cases. These are pure/core and compile checks, not complete runtime UI validation. No rebuild was performed for this checkpoint.

## Rollback

Latest verified backup, outside plugins:

~~~text
deployment-backups\installed-0.3.0-before-0.3.1-20261006T215707Z\Alexander.CharacterSheet.dll
deployment-backups\installed-0.3.0-before-0.3.1-20261006T215707Z\RESTORE.txt
~~~

Backup 0.3.0: **167,936 bytes**, reverified 2026-10-07:

~~~text
SHA256: 9EB121AD7C2808645258A2EF5359CDE538F5E0C9BEA7FD3C8DF84C00B78750C4
~~~

Earlier rollback also remains at deployment-backups\installed-0.2.2.5-before-0.3.0-20261006T203643Z, with its own RESTORE.txt. Additional earlier backups remain in deployment-backups. Verify the selected backup's documented version/hash before use.

Only restore when requested and after freshly confirming the game is fully closed. Copy only the selected custom DLL to the actual host target above; keep backups outside BepInEx\plugins. Follow RESTORE.txt, then verify actual-host size/hash and one custom DLL. Leave loader/config/bootstrap files unchanged. Do not launch or terminate the game automatically.

## Known limitations and useful next steps

General Cooldown Reduction is **Unknown** because no reliable single general-total adapter is bound. Separate known shared-item references exist; skill-specific cooldown values are excluded. Full health/damage/regen/armor source attribution remains partial; conditional damage is not combined into a universal multiplier.

Quantitative current/next-copy coverage is **23 registered item definitions**. There are **44 categorical routing definitions**; that does not imply 44 complete formulas. Other/modded effects remain honestly unknown. Full sprint base/source attribution, buff per-stack remaining timers and exact multiplayer countdowns are not promised.

Before treating 0.3.1 as fully validated, obtain specific user checks for:

- Long inventory scrolling, footer separation, label clipping and native icon/font/opacity appearance at their scale.
- Keyboard/controller focus entry, collapse recovery, scroll-to-focus and fixed Luck hover.
- Both native inventory tooltip routes, pointer transfer, crowded-edge placement and one-copy/multi-copy Details.
- Nectar gaining/losing eligible types, stack versus type counting, self-counter exclusion, over-cap count, changing item capacity and active/inactive bonus.
- Meaningful small stat bonuses alongside the cleaned Regen hover.
- Repeated Tab/menu transitions, focus/tooltip cleanup and single-player pause restoration.
- Multiplayer gameplay-click suppression and cleanup, if a multiplayer test is available.

On resume: read this checkpoint, verify actual installed DLL still matches 0.3.1, collect the user's specific observations, and choose the next requested issue. Start new source work from a copied visual-build\0.3.1 successor. Review separate inventory/buff features individually; do not blindly merge or deploy another candidate. Run meaningful relevant checks, record a reviewable DLL identity, then obtain deployment authorization and a fresh game-closed check before replacing only the host custom DLL.

The separate Game Night bot task must not be mixed into this repository.
