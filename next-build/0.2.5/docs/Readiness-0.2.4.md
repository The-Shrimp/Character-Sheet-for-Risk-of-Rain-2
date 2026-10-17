# Readiness — isolated 0.2.4 robustness patch

Verified 2026-10-05T21:04:22.342494+00:00. This patch is compiled and **unstaged**. Installed 0.2.2 and reported 0.2.3 remain frozen.

- **476 synthetic assertions pass**, including visible omitted regen sources, unknown Slug activation, separate/unknown protection pools and missing whole stat observations.
- Plugin compilation succeeds against unchanged local references; identical-input deterministic builds match.
- Patch DLL: artifacts/review-native-tab/Alexander.CharacterSheet.dll; **140,288 bytes**, SHA256 **906F140A4EADDF0C4BF886DF9FFEC4203208AB0A4F6E98B636761934BFFF7822**.
- Reported 0.2.3 remains **679D985D106FC21C17606DACFF61D69C4C13C0C931BF1FA528A40478EF9376F9**, **139,264 bytes**, with exact reported source hashes preserved.
- Installed 0.2.2 remains **FE8B520259EE94D91589961B39D1644798E4A141182EE53D86E4EF5460E5D0CC**, **109,056 bytes**.
- All 36 mapped root-source files, 145 external references and 7 protected config/loader/bootstrap/license entries match prior hashes. Five native presentation architecture files remain unchanged; Plugin.cs changes only the version string.
- Frozen 0.2.3 source: C:\Users\heale\Desktop\Not School Stuff\Risk of Rain 2 Stat Sheet\Character Sheet for Risk of Rain 2\snapshots\next-0.2.3-frozen-20261005T210031Z\source. Source ZIP SHA256 **6FCE068C87CEA03A3166F215950C8D3D411086E556A8B6704CDFCA54F140862E**; no binary entries. An archived copy of our own 0.2.3 DLL matches the reported hash; no game/vendor binary was copied.

[Machine-readable verification](../artifacts/next-build-verification.json), [freeze location](../artifacts/frozen-0.2.3-location.json), [model/public-source limits](CoreStatModel-0.2.4.md).

## Concrete patch behavior

Short stat hovers name active Knurl/Slug base rates omitted because raw scaling is unregistered. Unknown Slug activation stays explicitly omitted; inactive Slug does not display a false active-rate notice. Shield/barrier hovers now show current/max health and shield plus separate barrier. Unknown shield inputs remain unavailable rather than inferred from health. Missing master luck no longer defaults to zero; missing whole live stat observations cannot claim authoritative values.

No new exact conversion/scaling formula, item coverage or native UI architecture was added. The existing 37 partial item definitions and eight bounded buff states are unchanged.

## Remaining blockers

No Unity/runtime test has run. Installed 0.2.2 remains the first user test candidate; reported 0.2.3 and this patch are not installed. Knurl/Slug raw regen placement and Transcendence complete health/shield conversion remain unregistered. Other stat sources/hooks/floors/cached payouts stay incomplete; a matching residual does not prove complete attribution. Remaining buff timers are unavailable.

No game/profile/bootstrap/configuration/power/security changes or game launch occurred in this patch pass. The connected tool session was available during verification; no continued wake/connection guarantee is made.
