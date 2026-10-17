# Test-candidate readiness 0.2.2

The reviewed candidate is staged in AlexanderCharacterSheet-Dev. This is not runtime validation.

- Staged 109,056 bytes match candidate SHA256 FE8B520259EE94D91589961B39D1644798E4A141182EE53D86E4EF5460E5D0CC.
- The original profile DLL is preserved outside plugins with a nonloadable .bak suffix; old hash D70F964C01EC5D929A3AA29F14D77EB9B0CC3F7A63F9C93325D2BDA5B16E208C. Restore path/instructions are recorded in staging-verification.json.
- Exactly one loadable custom DLL remains. Only that DLL was replaced; config, checked BepInEx/Harmony references, game bootstrap files and existing license matched before/after.
- No dependency/config migration was needed. The existing scale is 1; General.Enabled is absent and defaults true on future startup. Preserved Input entries are unused by 0.2.2.
- 366 synthetic assertions passed and local compilation succeeded. Definitions cover 23 explicitly partial item IDs and eight buff names/states; SourceCoverage.md lists all supported/unregistered terms.
- Nectar's fixed-type next benefit can be zero; Beads use stored level-gain equivalents; Sale Star remains reported-bug/unverified with no adjusted chance.
- Stats attribution remains incomplete. War Horn magnitude, Shrimp shield capacity, Conductor buff identity, actual attack conditions and all remaining timers remain unregistered.
- Source lifecycle/ownership gates retain native player/inventory UI. Native providers/shared assets are not written.
- The existing loader core matches build-reference bytes but is not the full current Thunderstore dependency bundle. Runtime bootstrap/Mono/Harmony/game compatibility is untested.
- No own dependency or required config migration blocks basic appearance smoke identified at preflight. Native cursor accessibility, own-strip/font/canvas/target-method runtime behavior remain untested prerequisites. Controller support remains unfinished.
- Game and manager were closed during/after staging. No game launch, accounts/saves/Steam/global-manager changes, new game implementation inspection or publication occurred.

TestCandidate-0.2.2.md and ManualSmokeChecklist.md give user-run launch, smoke and recovery. Exact evidence is artifacts/review-coverage/staging-verification.json. The manager's earlier package label may differ from the staged DLL version because profile metadata was preserved.
