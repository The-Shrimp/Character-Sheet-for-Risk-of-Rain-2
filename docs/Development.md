# Development evidence and remaining validation

## Selected repository and boundaries

Implementation stays in C:\Users\heale\Desktop\Not School Stuff\Risk of Rain 2 Stat Sheet\Character Sheet for Risk of Rain 2, inside the user's selected parent folder. Source writes use narrowly scoped reviewed sandbox escalation. No alternative implementation checkout was created.

No existing game binary, Steam metadata or power setting was modified. Three new standard loader bootstrap files were added at previously absent game-folder paths after clarified setup approval; Doorstop is disabled by default. With explicit user approval, official r2modman3.2.20 was installed per-user and a new isolated AlexanderCharacterSheet-Dev profile was created. No preexisting mod profile was edited and no game was launched. The user reported a five-hour sleep setting; that setting was not changed or independently verified after their report.

## Local API evidence

Reflection-only inspection of the installed RoR2.dll (Steam build 21587608) established:

- RoR2Application.isInSinglePlayer checks server activity, dontListen, and excludes local multiplayer. Player-count heuristics are not used.
- PauseStopController.Pause(bool) is the native pause request. Its implementation preserves and restores the game's prior time scale. The mod reads the time scale only to detect a preexisting external pause.
- HUD.DeactivateScoreboard requests Pause(false) on a single-player path. A scoped Harmony guard protects a borrowed pause during that native method.
- HUD.SetScoreboardActive/SetScoreboardState and the mapped info action are the native screen controls.
- MPEventSystem.cursorOpenerCount is available. The sheet adds/removes only its own opener.
- LocalUser.cachedBody is the own-body source; no remote body is read.
- Inventory.GetItemCount is an obsolete alias of GetItemCountEffective. Modern permanent/effective counts are read explicitly.
- BuffDef.isHidden, isCooldown, ignoreGrowthNectar, canStack and BuffCatalog.ignoreGrowthNectarIndices are available. Nectar excludes its own BoostAllStatsBuff counter and counts qualifying types, not stacks.
- CharacterBody.timedBuffs is private. TimedBuff.timer is remaining local duration; GetTimedBuffTotalDurationForIndex is server-only and returns total duration, so it is not misused as a countdown.
- Util.CheckRoll takes CeilToInt(Abs(luck)) extra rolls. Positive luck uses the minimum draw; negative luck the maximum. Tests include fractional rounding.
- CritGlasses contributes 10 points per effective copy to crit before later conversions.
- FlatHealth contributes 25 per effective copy before shared health scaling and later multipliers/conversions.

tools/InspectGame.cs never executes game assembly methods. The Framework reflection runtime cannot represent some netstandard2.1 game types; it reports those rather than silently treating reflection failures as missing APIs.

## Approved setup performed; runtime remains unverified

The official r2modman3.2.20 installer was downloaded from ebkr/r2modmanPlus and its SHA256 matched the GitHub release asset digest:
`a6d1e0fa80b3bbd3a792bcd19321d82fe346cc0a59ce76d935fc958f75499636`.
It installed successfully at `C:\Users\heale\AppData\Local\Programs\r2modman` (installer exit0, executable product version3.2.20.0). No account was created.

Official BepInExPack5.4.2122 was downloaded. BepInEx.dll5.4.21.0 and Harmony2.9.0.0 were extracted to ignored .references for build only. The full plugin compiles cleanly with the existing Roslyn compiler; no SDK was installed.

The approved new profile is at `C:\Users\heale\AppData\Roaming\r2modmanPlus-local\RiskOfRain2\profiles\AlexanderCharacterSheet-Dev`.
Only the archive's core loader files, our compiled plugin, local configuration and explicitly local mods.yml entries were staged. It is a core-only local test installation, not the complete Thunderstore dependency bundle. No MiscFixes, SeekersPatcher, BepInEx_GUI, FixPluginTypesSerialization or RoR2BepInExPack was installed. Runtime compatibility of this minimal combination is unverified. Do not silently update it into the complete dependency bundle.

The official manager source uses directories as local profiles and accepts JSON in mods.yml through its YAML parser. The running manager recognized and selected the exact dev profile and both local installed entries. Its new local Steam/game paths, default game/store, selected profile/platform and linked-file paths were configured via the local renderer IndexedDB schema. Native game computer-use screenshot/input tooling remains unavailable; the official @oai/sky package does not resolve in this executor.

**Launch boundary:** official ModLinker.ts copies profile root bootstrap files (.doorstop_version, doorstop_config.ini, winhttp.dll) into the Steam game directory. The manager excludes BepInEx subdirectories from those copies. This standard linking was approved by the parent clarification and performed for exactly the three root bootstrap files. All three target paths were absent, so no existing file was overwritten. The absence/hashes are recorded in ignored artifacts/bootstrap-link.json. The profile and game-folder Doorstop configuration were set/verified disabled by default. No game launch was performed; this is configuration evidence rather than a runtime vanilla-launch test.

Sources:
- https://thunderstore.io/c/riskofrain2/p/ebkr/r2modman/
- https://thunderstore.io/c/riskofrain2/p/bbepis/BepInExPack/required
- https://github.com/ebkr/r2modmanPlus/blob/v3.2.20/src/r2mm/manager/ModLinker.ts
- https://github.com/ebkr/r2modmanPlus/blob/v3.2.20/src/r2mm/mods/ProfileModList.ts
- https://github.com/Wet-Boys/LookingGlass/ (LGPL-3.0 reference only; no code copied)

## Runtime hold and deferred live checks

Runtime/new external mutations and further game-code inspection are now held for unresolved terms/account permission. No further game assets/code will be extracted into model context. A separate in-game profile does not isolate Steam achievements, and no fallback save/device/account workaround is proposed. No in-game test profile or run was created. The listed tests remain deferred and do not authorize launch.

1. Solo open/close, hold/toggle configuration, cursor toggle, Shift tooltips, low/high resolution and UI scaling.
2. Opening from an already paused menu, external pause/resume while open, rapid close before pause acknowledgment, Escape/menu takeover.
3. Death, scene change, run exit, focus loss, plugin disable: release only owned input/pause; retain borrowed pause.
4. Multiplayer host and client: verify no pause requests; client buff countdowns remain unavailable.
5. Compare shown totals with CharacterBody values and native descriptions; exercise crit conversions, temporary/suppressed items, Nectar flags/counters, and unsupported items.
6. Validate font, row/tooltip clipping, scrolling and hover stability. Source compilation is not visual validation.

No commit, publication or runtime-ready claim has been made.

Own-source audit and the factual action inventory are saved separately. Source-only tests pass41 assertions. New component math/late calculation stages have not been integrated into the live presentation or rebuilt/staged.

## Independent original UI-core follow-up

After the diagnostic manager was closed, work continued only within the selected source repository. NumberFormatting, ComparisonPresentation/AttributionPresentation, InventoryPresentation and TooltipPresentation contain no Unity/RoR2 references or game-specific formulas. Synthetic tests exercise supplied fake data. See PresentationCore.md.

No further game inspection, native game automation, game build, installation, profile/bootstrap update, publication or new system preference change occurred in this follow-up. The prior staged plugin remains unchanged; independent integration and pixel layout are unverified.
