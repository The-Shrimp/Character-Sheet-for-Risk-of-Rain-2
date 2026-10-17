# Character Sheet for Risk of Rain 2

A local-character UI mod for Alexander. Source milestone 0.2.2; compiled locally and not validated in game. The reviewed 0.2.2 test DLL is staged in AlexanderCharacterSheet-Dev; the previous DLL is backed up outside plugins.

The source now extends the native Tab scoreboard without replacing its player/inventory structure. It adds a compact live-stats panel, collapsible grouped effects and active-buff glossary, and enhanced hovers on your native inventory icons. It references the native TMP font/material and a native background sprite at runtime; no proprietary assets are extracted or bundled. No dashboard tabs or search bar appear by default.

Native mapped key/cursor behavior is retained. Single-player pause ownership distinguishes a preexisting external pause from the native Tab opening, and multiplayer never requests pause. The mod reads Time.timeScale only as a pause observation. See docs/NativeTabImplementation.md for source coverage and exact runtime gates.

## Status and honest limits

- 366 synthetic assertions pass, including fixed n=1/4/10 current/next item expectations, caps/proc/luck, Nectar and unknown-state checks. See docs/SourceCoverage.md and docs/Readiness-0.2.2.md. The review plugin compiles against the existing authorized local references.
- Full BepInEx/Harmony compilation passes using the existing BepInExPack 5.4.2122 core references. No game tests were run. The candidate is staged in the dev profile; native screen/input and loader runtime validation remain pending. Compilation does not establish runtime compatibility.
- Formula coverage remains partial: 23 item IDs have source-qualified component terms and next-copy comparisons. Eight buff names/states have explicit bounded descriptions. All unregistered effects retain unknown labels; remaining buff timers are unavailable. See docs/SourceCoverage.md for the exact supported/unsupported list.
- Stat attribution shows survivor baseline, level growth and the known crit term, with an explicit residual/incomplete label. Full buffs, interactions, conversions, and other mods are not yet attributed.
- Buff definitions expose stacks, Nectar eligibility and cooldown flags. Remaining durations are explicitly unavailable pending a public per-stack timer adapter; private timedBuffs reflection was removed. Glossary effect coverage is limited and unknown effects are labeled.
- Own character only. Hidden items/buffs, enemies, loot and remote-player bodies are excluded. Inventory sorting changes presentation only.
- Mouse and keyboard first; controller navigation has not been confirmed or implemented.

## Build without installing an SDK

Use the already installed Roslyn compiler via PowerShell:

    .\build.ps1 -Mode CoreTests
    .\build.ps1 -Mode GameApi
    .\build.ps1 -Mode Plugin -LoaderCorePath 'PATH\TO\REVIEWED\core'

The final mode requires BepInEx.dll and 0Harmony.dll. References remain local and are not copied into the output. Output goes to artifacts; the script does not download, install, deploy, launch the game or edit a profile.

CharacterSheet.csproj is included for future IDE/SDK use. It is not required by build.ps1. Do not install an SDK just to run the provided compiler path.

## Runtime configuration

BepInEx entries: Input.ToggleSheet, Input.OpenCursor, Input.CursorToggleKey (Left Alt), Presentation.Scale (0.7–1.2). Profile settings are not edited. The game retains its mapped info/Tab action.

r2modman3.2.20 is installed. The isolated AlexanderCharacterSheet-Dev profile is recognized and selected in the manager before it was closed with two local entries (core-only BepInEx and our compiled DLL). Local Steam/game paths are configured. Three standard bootstrap files were added to previously absent paths in the game folder, with Doorstop disabled by default. No existing game binary was replaced and no game was launched. No complete Thunderstore dependency bundle was installed.

**Runtime hold:** do not launch, add dependencies, change profiles/bootstrap files, or remove installed pieces until the parent/user review clears the hold. No achievement isolation is claimed. No in-game test profile, save backup or fallback device was created. Read the factual computer action inventory in docs/ActionInventory-2026-10-05.md.

The new verified component math and late calculation stages are tested in source but have not been wired into the live UI or rebuilt into the staged DLL. The UI coverage remains the preliminary two-item registry described above.

See docs/Development.md for API evidence, setup boundaries and live validation requirements.

## Independent UI-core milestone

Original game-independent modules now cover compact/full numeric precision, uncertainty-preserving current/next comparison text, explicit attribution discrepancy, immutable inventory row models, deterministic sorting/filtering, mixed luck labels, stable selection and bounded plain tooltips. They receive supplied values and metadata and add no item/buff formulas.

The original item-tooltip model and number formatting are now wired into the source adapter and separate review build. Complete sorting/filtering/attribution adapter coverage is still pending. The staged DLL was not rebuilt or replaced. During the terms/account hold, CoreTests and PresentationCore modes build only original standalone code. PresentationCore produces artifacts/Alexander.PresentationCore.dll from the five independent modules. The parent authorized local reference-only PluginReview compilation to artifacts/review-native-tab. Installed/profile/bootstrap changes and game launch remain held.


## Standalone design preview

The original sample render is artifacts/design-preview/character-sheet-preview.png (1600 x 1160), with prepared HTML beside it. Source is in preview/character-sheet-preview.html and preview/PreviewData.cs. The prominent banner reads "Design preview — sample data, not an in-game capture".

Prayer Beads uses the user's supplied equation: Stores 35% (20 + 5 x each additional item) of level-up stat gains. Four total sample beads include three additional copies; the fifth-copy label is40% (+5 percentage points). This arithmetic validates the supplied example only, not game mechanics. Other item names, all character values and geometry icons are synthetic. The delivered version1 PNG is the temporary dashboard study. The revised source-only schematic retains a conceptual Tab scoreboard/player/inventory region with additive stats and item hovers; it removes dashboard tabs and default search/sort/filter controls. This revision was not rendered or uploaded.

The new ItemTooltipPresentation template and24 synthetic assertions distinguish total and additional copies, keep scaling inline, and handle unknown/nonfinite/overflow values and scoped luck labels. The live adapter/staged plugin remains unchanged. The preview was rendered with the existing local headless Chrome browser, a project-contained profile and no remote debugging.


## Native Tab review build

    .\build.ps1 -Mode PluginReview -LoaderCorePath '.references\BepInExPack-5.4.2122'

This compiles the source-only native integration to artifacts/review-native-tab/Alexander.CharacterSheet.dll. It does not overwrite the earlier artifact or deploy any file. See docs/NativeTabImplementation.md. Compile success is not runtime validation or a compliance guarantee.


## Source quality milestone0.2.1

See docs/Readiness-0.2.1.md for the concise report, docs/SourceCoverage.md for exact supported IDs/fallbacks, and docs/ManualSmokeChecklist.md for later profile selection and UI recovery. This build retires old context on respawn/HUD/controller changes, prevents duplicate attachment/hook installation within its source instance, and routes eligible local hovers without writing native tooltip-provider fields.247 synthetic assertions and isolated local compilation pass; Unity/game-only tests remain unrun.

AtG Missile Mk.1 now has a conditional publicly sourced damage-component current/next tooltip with explicit unknown actual attack chance. Sale Star's extra-item luck interaction has a reported-bug/unverified label and no confident adjusted probability. The earlier staged DLL/profile/game files remain unchanged.

## Coverage review 0.2.2

The original registry now has 23 partial item definitions and eight explicit buff names/states. Each supported term has current-stack/next-copy math with source-qualified conditions; this does not make stat attribution complete. See docs/SourceCoverage.md and docs/Readiness-0.2.2.md. The isolated review DLL is artifacts/review-coverage/Alexander.CharacterSheet.dll. The reviewed DLL is now staged in the existing dev profile, with an old-DLL backup. Unity/game tests remain unrun.

## Staged test candidate

Verified 0.2.2 / 109,056 bytes is staged in AlexanderCharacterSheet-Dev, matching the reviewed FE8B5202... SHA256. Only the custom DLL changed; config/loader/bootstrap checks matched. The old profile DLL is backed up outside plugins. See docs/TestCandidate-0.2.2.md and artifacts/review-coverage/staging-verification.json for launch/recovery and exact hashes. This is an unvalidated test candidate, not a finished runtime-tested mod.
