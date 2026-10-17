# Own-source audit — 2026-10-05

This is a review of our authored C# source, not a runtime or legal certification. No game launch occurred. Further game-code inspection and external setup mutations remain held.

## Calls found

- Reads own `LocalUser.cachedBody`/HUD and own effective/permanent inventory counts. No inventory give/remove/reorder calls occur.
- Reads live stats and active buff data; no stat setters, `RecalculateStats`, Add/Remove/SetBuff calls or damage/skill activation calls occur.
- Uses strict `RoR2Application.isInSinglePlayer` before issuing native `PauseStopController.Pause(bool)`. Multiplayer paths do not acquire/release pause. The time scale is read for preexisting-pause detection and never written.
- Changes only its own UI tree, native scoreboard toggle/display state and its own contribution to local cursor opener count. Focus/scene/death/disable cleanup exists. Borrowed native pause is protected by the scoped scoreboard-close Harmony guard. These lifecycle claims still require live validation.
- Reads configured keys through `Input.GetKey/GetKeyDown`; does not synthesize movement, attack, pickup or other gameplay input.
- `NetworkServer.active` is a read-only availability check for local buff timers. No network sends, RPC/Cmd invocation, sockets or HTTP clients occur in the mod.
- No achievement, progression, unlock, UserProfile, save, Steam/Cloud/account APIs or persistent `File`/`Directory`/`PlayerPrefs` calls occur in the authored mod source.

## Persistent side effects and limits

BepInEx `Config.Bind` is used for four mod settings; the loader may persist those in the selected profile's `BepInEx/config/alexander.charactersheet.cfg`. BepInEx logging may persist log output. Therefore the plugin is read-only with respect to character/inventory/progression gameplay state; it is not described as making zero persistent loader writes. The game or Steam can independently save settings/progress/achievements when launched, which this source audit cannot prevent or certify.

The current staged35,328-byte DLL is the prior full compile. The subsequently added tested component helpers and ordered late stages are source only; no new DLL was staged after the hold.

No guarantee about third-party loader binaries, other mods, game account effects, terms permission or actual UI/pause behavior follows from this source review. No removal, save backup, fallback device or account workaround is authorized by this document.
