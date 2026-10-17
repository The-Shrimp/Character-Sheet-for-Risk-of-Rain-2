# Manual native UI smoke checklist

This is a later user-run manual test plan. No steps were executed by this task. The reviewed 0.2.2 DLL is now staged in the existing AlexanderCharacterSheet-Dev profile, with a verified old-DLL backup outside plugins. Compilation and staging do not establish loader/runtime compatibility. See TestCandidate-0.2.2.md for exact controls and current launch/recovery steps.

## Choose the existing isolated profile

1. Open your installed r2modman manually and select Risk of Rain 2 with the Steam store.
2. At profile selection, choose AlexanderCharacterSheet-Dev. This is the separate profile already created for this project, not another existing profile.
3. Check that its installed list identifies the local BepInEx core and Alexander Character Sheet entry. Record the actual startup-log plugin version 0.2.2; the manager package label may retain the earlier import version.
4. Use Start modded only when you choose to run this manual smoke. This checklist does not copy DLLs, add dependencies, enable Doorstop or change Steam settings. No automatic test run is configured.

The profile separates mod files. It does not isolate the Steam account, achievements, saves or Cloud state. No save/account changes or fallback-device workaround are part of this plan.

## First solo check

- Use the game's mapped info/Tab control normally. Retain its existing hold/toggle behavior and all original player/inventory entries. No extra dashboard navigation or search should appear.
- Look for a compact stats panel outside existing native player panels. On crowded/unsupported geometry it should stay hidden and log its status while retaining native UI.
- Hover a local item, then move into the enhanced popup and bottom-right clover. Shift expands math/conditions; release Shift to return. Scroll longer details. Move off the popup, close Tab and reopen repeatedly: no lingering/duplicated popups.
- Hover native other-player entries only during a later multiplayer check: they must keep native behavior, with no new remote inventory/body readout.
- For naturally owned supported items, compare effective/permanent counts and component values. Prayer Beads four-copy 35% and next 40% is conditional; AtG coefficient is separate from actual trigger chance/DPS. Unsupported next-copy benefit must remain unavailable.
- Sale Star's extra-item luck interaction must show reported-bug/unverified. It must not promise a Clover-adjusted probability. Confirmed unaffected labels apply only to named scopes.
- For Nectar, compare its authoritative current/max counter with the metadata-derived reference. A next copy below capacity can show zero immediate modeled bonus; types count once even when their stack count grows.
- For Beads, read stored counter estimates as survivor level-gain equivalents, not a percent of current stats or a promised cached payout.
- Compare distinct Mocha terms, Plate versus armor, Raincoat maximum versus ready charges, Antlers active-orb versus full-stack maximum, and Conductor ideal uptime versus actual pulse conditions. Nominal buff duration must never appear as remaining time.
- Expand the grouped effects/buff glossary. Confirm stack/type/counter distinctions, explicit unverified effects and unavailable timers; Nectar counter must exclude itself.
- Check smaller/larger resolutions and Presentation.Scale. Text, scrolling and the native inventory remain usable.

## Lifecycle and pause checks

- In solo, ordinary Tab opening/closing should acquire/adopt/release pause once. No direct time-scale write is used.
- If the game allows Tab while already paused, closing it must retain the preexisting pause. Resume using the game's native pause controls when you intend to resume.
- Open the full pause/menu while the addition is visible, lose/regain focus, and close/reopen the native scoreboard. The added UI should retire cleanly without owning the native cursor or key state.
- During an ordinary run, observe death, respawn/body replacement, stage transition and return to menu. The old item/popup scope must retire; a valid new own-player panel can acquire one fresh scope.
- After approved multiplayer testing is available, check both host/client: no mod pause requests and no promised client countdown. Keyboard/controller focus and other UI-mod coexistence remain untested until these checks.

No gameplay-changing item grants, cheats, automated input, hidden loot/enemy display or save manipulation are needed for these checks.

## Recover if the UI becomes stuck

1. Release the mapped Tab/info control and Shift, then move the pointer away from the popup.
2. Switch focus to another window and back. The mod's focus-loss handler is intended to retire additions and release only its acquired pause; this recovery has not been runtime-tested.
3. Use Escape/the game's normal pause and resume/close controls. A pause that existed before Tab must be resumed by your own native action.
4. If you cannot restore normal control, quit through the game's normal exit flow when available. Do not keep testing the same failure.
5. With the game closed, select AlexanderCharacterSheet-Dev in r2modman and disable the Alexander Character Sheet entry for the next run. Leave unrelated profiles, loader files, Steam/account and save settings alone.
6. The staged build's General.Enabled=false setting is another manual disable option for that profile on a later startup. It was introduced in source 0.2.1 and is present in 0.2.2; the old 0.1.0 backup does not provide it.
7. Preserve the failure log/screenshot and note the selected profile, plugin version, solo/host/client state, action and resolution for review. Do not share unrelated account/session files.

Do not delete game binaries, bootstrap files, saves or caches as a UI recovery step. This plan makes no guarantee that a stuck game can recover without exiting.
