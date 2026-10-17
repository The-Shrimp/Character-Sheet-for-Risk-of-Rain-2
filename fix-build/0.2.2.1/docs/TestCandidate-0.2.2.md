# 0.2.2 test candidate: installation, launch and recovery

The reviewed 0.2.2 DLL is NOW STAGED in the existing AlexanderCharacterSheet-Dev profile. The same originally denied DLL replacement was retried once after owner-authorization transcript evidence was supplied; automatic review approved it. Staged bytes and SHA256 match the reviewed candidate. No game/runtime test was run.

Only the exact custom DLL was replaced. The previous profile DLL was copied to a verified nonloadable project backup outside plugins. Configuration, checked loader/bootstrap files and existing license remained unchanged; no dependency packages or profile metadata were changed.

Candidate: C:\Users\heale\Desktop\Not School Stuff\Risk of Rain 2 Stat Sheet\Character Sheet for Risk of Rain 2\artifacts\test-candidate-0.2.2\Alexander.CharacterSheet.dll
Candidate SHA256: FE8B520259EE94D91589961B39D1644798E4A141182EE53D86E4EF5460E5D0CC
Intended exact profile destination: C:\Users\heale\AppData\Roaming\r2modmanPlus-local\RiskOfRain2\profiles\AlexanderCharacterSheet-Dev\BepInEx\plugins\Alexander-CharacterSheet\Alexander.CharacterSheet.dll
Old DLL SHA256: D70F964C01EC5D929A3AA29F14D77EB9B0CC3F7A63F9C93325D2BDA5B16E208C
Verified previous profile-DLL backup: C:\Users\heale\Desktop\Not School Stuff\Risk of Rain 2 Stat Sheet\Character Sheet for Risk of Rain 2\artifacts\backups\dev-profile-0.1.0-before-0.2.2-20261005T195808374Z\Alexander.CharacterSheet.0.1.0.dll.bak

This is a manual test-candidate bundle, not a Thunderstore-import package. Do not import it through r2modman or extract the whole bundle into the game/profile. The completed staging action copied only Alexander.CharacterSheet.dll to the exact destination above and verified the new hash. No additional DLL authored by us is required. The plugin source includes the original core/presentation code in this one DLL, with only game/framework and existing BepInEx/Harmony references.

## Launch when you choose to test the staged candidate

1. Open the installed r2modman:
   C:\Users\heale\AppData\Local\Programs\r2modman\r2modman.exe
2. Select Risk of Rain 2, Steam, then the existing profile AlexanderCharacterSheet-Dev.
3. Check the local Alexander Character Sheet entry is enabled. The manager's package label may still show the old import version because only the DLL is intended to change; confirm actual source version 0.2.2 in the startup log.
4. Use the manager's Start modded control when YOU choose to test. The default game Doorstop config remains disabled; manager integration is the intended profile launch path and is not validated here. No game has been launched by this task.
5. Begin an ordinary solo run with your own living character. The added panel is expected only while the native Info/scoreboard is visible, not in the title/lobby/character-selection menu.

The manager's documented launch path is Start modded:
https://github.com/ebkr/r2modmanPlus/wiki/Why-aren%27t-my-mods-working%3F
Only that launch guidance is used here; this kit does not prescribe loader fixes, package installation, Steam argument changes, security exceptions or validation.

## Current exact controls

- Open/close the game's mapped Info/scoreboard action, normally Tab. Its native hold/toggle setting applies. There is no separate sheet hotkey or independent mod menu.
- Hover your OWN native inventory icon or an added stat row. Move onto the popup and hover its bottom-right clover for the luck scope.
- Hold either Left Shift or Right Shift for full math/conditions; release it for the compact view. Scroll over long popup details.
- Click Effects & buff glossary to expand/collapse the added rows.
- Use the game's native pause/back controls (normally Escape) for normal game menus. Focus loss is intended to retire the additions.

The preserved old Input.ToggleSheet/OpenCursor/CursorToggleKey entries are UNUSED by 0.2.2. In particular CursorToggleKey=LeftAlt is not a current mod key. This source does not acquire/release a cursor or add a cursor toggle. It depends on the native scoreboard providing a usable pointer; that prerequisite has not been tested. If native Tab provides no usable cursor, hover/click testing is blocked until that behavior is reviewed. Do not assume an undocumented alternate key.

## First smoke and expected limits

- Retain all original player/inventory entries and native count/order. Look for the compact own-stat addition; it can hide when no free canvas region is available.
- Hover one naturally owned supported item. Current/next terms must be component estimates, not total damage/DPS or full attribution. Unsupported effects must remain unavailable.
- Close/reopen Tab, then move off/on the popup: check for duplicate/lingering UI. Check Shift and the clover popup.
- Use the effects disclosure for buff names, observed stacks, counter/charge distinction and metadata-based Nectar eligibility. All remaining durations are unavailable; nominal documented durations are not live timers.
- Nectar compares a fixed observed eligible-type set and may show zero immediate next-copy benefit. Its authoritative live counter remains separate.
- Prayer Beads counter math is stored level-gain equivalents, not percent of current stats or certified cached payout. Sale Star remains reported-bug/unverified with no adjusted chance.
- The registry has 23 partial item definitions and eight explicit buff names/states. War Horn magnitude, Shrimp shield capacity, full stat attribution, actual proc/target conditions, Conductor buff ID and unknown effects remain unregistered.
- Do pause/focus/death/respawn/stage and later multiplayer checks only as described in docs/ManualSmokeChecklist.md; these have not run.

There is no unresolved own-DLL dependency or required config migration identified for basic appearance testing. The existing scale is 1 and is compatible. General.Enabled is currently absent; the source binds its default true on future startup. No config was edited during staging. Native cursor availability, own-strip identification, font/canvas capture and Harmony callback installation are UNTESTED runtime prerequisites. Controller support is unfinished.

The profile has the existing BepInExPack core only, not the full current Thunderstore dependency bundle. Its BepInEx.dll/0Harmony.dll bytes match build references, but this does not validate bootstrap/Mono/patch compatibility with the game build. A loader error can precede this plugin. No new dependency packages were installed or recommended as an automatic workaround.

## If no UI appears

1. Confirm this is AlexanderCharacterSheet-Dev. Its custom DLL was verified as 0.2.2 / 109,056 bytes at staging; package labels can still show the earlier import version.
2. Use the selected profile's Start modded path and a living character in an active solo run; open the native scoreboard.
3. Inspect only the relevant profile BepInEx/LogOutput.log after that test. Look for Native Tab-extension source 0.2.2 and the plugin's status/error. A missing startup message may indicate loader/disabled-plugin failure.
4. Source status can say Native own-player strip is not ready, Native TMP font/canvas is unavailable, or No free canvas region for stats. The last preserves native UI and may still allow item hovers.
5. If the cursor is inaccessible, report that exact native Tab behavior. Do not change game/bootstrap/global settings or add guessed hotkeys/packages to chase the issue. Keep the selected profile, plugin version, action and resolution for review.

## If control/pause/UI becomes stuck

1. Release Info/Tab and Shift; move the pointer off the popup.
2. Switch focus away and back. Cleanup is intended but untested; do not count on recovery.
3. Use the game's native pause/back/resume controls. A pause that predated Tab is intended to remain until your own native resume.
4. Quit normally if possible and stop testing that failure. If the game is wholly unresponsive, normal Windows force-close may be necessary and can lose unsaved run progress; no guaranteed recovery is claimed.
5. With the game closed, use this profile's manager to disable only Alexander Character Sheet for the next run. Do not delete loader/game files, saves or caches.
6. If an approved restoration is wanted, close both game and manager and follow the verified RESTORE.md in the project backup directory above. Keep one loadable custom DLL only. General.Enabled=false in the plugin's own config is another manual later-startup disable option after that entry exists.

This profile separates mod files, not the Steam account, achievements, saves or Cloud state. This task changed none of those. Preserve only the relevant mod log/screenshot; unrelated account/session files are not needed.
## Verified staging evidence

Staged UTC: 2026-10-05T19:58:08.7062385Z

Staged bytes: 109056

Staged SHA256: FE8B520259EE94D91589961B39D1644798E4A141182EE53D86E4EF5460E5D0CC

Old backup SHA256: D70F964C01EC5D929A3AA29F14D77EB9B0CC3F7A63F9C93325D2BDA5B16E208C

Restore instructions: C:\Users\heale\Desktop\Not School Stuff\Risk of Rain 2 Stat Sheet\Character Sheet for Risk of Rain 2\artifacts\backups\dev-profile-0.1.0-before-0.2.2-20261005T195808374Z\RESTORE.md

Full verification: artifacts/review-coverage/staging-verification.json. There is exactly one loadable custom DLL; game/manager process count after staging was zero. No game launch, save/account change or runtime UI validation occurred.
