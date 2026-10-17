# Computer action inventory — checkpoint 2026-10-05 16:47 UTC

Runtime and further game-code inspection are on hold for the parent/user terms and account-safety review. No game launch occurred. No in-game test profile or run was created. No gameplay input was sent. No Steam account, achievement, Cloud, save or security settings were changed. No Steam global launch options were changed. A separate in-game profile would not itself isolate Steam achievements; no such isolation is claimed.

## Completed changes outside the selected source repository

1. Downloaded the official r2modman3.2.20 Windows installer to `C:\Users\heale\AppData\Local\Temp\ror2-approved-setup`. Its SHA256 matched the official release digest. The installer is unsigned; no certificate/security bypass or Unblock-File was performed.
2. Installed r2modman per-user at `C:\Users\heale\AppData\Local\Programs\r2modman`; installer exited0 and installed executable reports3.2.20.0. No account was created. The official installer/application performs standard installer/protocol integration; its complete registry/shortcut footprint was not independently diffed, so no claim of zero Windows changes is made.
3. Downloaded official BepInExPack5.4.2122 into the same temporary setup directory. Core loader files were staged into the new local profile. No MiscFixes, SeekersPatcher, BepInEx_GUI, FixPluginTypesSerialization or RoR2BepInExPack dependency was installed. The core-only runtime has not been validated.
4. Created `C:\Users\heale\AppData\Roaming\r2modmanPlus-local\RiskOfRain2\profiles\AlexanderCharacterSheet-Dev`. It contains core loader files, local `mods.yml`, our compiled DLL, MIT license and `BepInEx\config\alexander.charactersheet.cfg`. Existing profiles were not edited.
5. Started only r2modman, with Electron's documented debugging interface bound to `127.0.0.1:9334`. Its actual page recognized Risk of Rain2, Steam, the dev profile and two local installed entries. No Start modded action was taken.
6. Wrote only r2modman's new local settings: Steam directory `C:\Program Files (x86)\Steam`, game directory `C:\Program Files (x86)\Steam\steamapps\common\Risk of Rain 2`, default game RiskOfRain2/default store steam, selected profile AlexanderCharacterSheet-Dev/last platform steam, and the three linked bootstrap paths. These are manager preferences, not Steam launch options. IndexedDB's `settings` database was used through the manager's local renderer; no account/session data was read.
7. Changed the new profile's Doorstop default to `enabled=false`, then linked three standard bootstrap files to the Steam game directory: `.doorstop_version` (5 bytes), `doorstop_config.ini` (1567 bytes), `winhttp.dll` (24576 bytes). All three target paths were absent before linking, so no existing files were replaced. No game binaries were patched/replaced and no BepInEx/plugin directory was copied into Steam. No gameplay is claimed unaffected based on a runtime test; the default-disabled configuration and explicit manager launch override were verified from files/source only.
8. Wrote temporary own inspection/configuration scripts and metadata/IL diagnostic text in `C:\Users\heale\AppData\Local\Temp\ror2-api-preflight` and `ror2-approved-setup`. Proprietary game assemblies/assets were not copied to a repository, uploaded, or published. Read-only game assembly/IL inspection had occurred before the new hold; it has now stopped. Existing diagnostic text is preserved, not deleted.

## Source repository and backup record

The only implementation checkout is `C:\Users\heale\Desktop\Not School Stuff\Risk of Rain 2 Stat Sheet\Character Sheet for Risk of Rain 2`.
Source/UI/build/tests/docs were added there; README and .gitignore were updated. No commit or publication occurred. Ignored .references contains the two downloaded BepInEx/Harmony build references; artifacts contains compilation outputs.

`artifacts\bootstrap-link.json` records the pre-link absence and linked SHA256 of all three files. Since none existed previously, no original-file backups were necessary or created. No save backup or fallback game profile/device was created. No data was deleted.

Linked hashes at this checkpoint:

- `.doorstop_version`: B4116D6E880009DC1440DDAB7EC054BCEA529AEA394EC5BAB7943B415A359281
- `doorstop_config.ini`: D9DAB1EB8E541971D5F6ED0DF139F30706E77615E06847FFBBB41B174B8D9C0F
- `winhttp.dll`: 7E90398DD7154B41F76E0524AEDB795A7BCBE2ED9A14E4CEEE214235AA04E442

## Observed current processes and test limitation

At16:47:37UTC, no RiskOfRain2 process existed. Steam was already running (PID26636). r2modman main PID106384 and child PIDs108324,110632,114596 existed. Local port9334 was bound only to127.0.0.1 byPID106384. No process was killed or game closed. New external mutations, including manager cleanup/restart or bootstrap removal, are held pending review.

The official native Computer Use runtime is unavailable on this executor: no screenshot/input/node_repl tool was exposed, and importing `@oai/sky` returned ERR_MODULE_NOT_FOUND. Electron manager DOM/config inspection succeeded, but that does not provide game screen/input control. No screenshots or visual-game test results are claimed.

Core source tests and compilation do not establish runtime compatibility or account/terms compliance. Future test instructions remain a review checklist, not approval to launch.
