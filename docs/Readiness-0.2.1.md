# Source readiness 0.2.1

Ready for source/build review; not runtime-certified or deployed.

- Original native Tab additions compile to the separate review DLL.
- 247 synthetic assertions pass: 54 new quality/coverage assertions cover session replacement/cleanup, exclusive installation and binding, stale deferred teardown, controller-scoped pause protection, formula provenance/unknown context, buff eligibility/counter distinction and uncertain/reported-bug chance suppression.
- Native tooltip providers, their tokens/enabled flags and shared style resources are not written. Local hover routing applies only while an eligible icon has our exclusive active claim. On detach or unavailable context, native handling falls through.
- Public IPointerEnterHandler implementation was proved by a never-executed compiler probe. The explicit callback is resolved through the public interface map at future startup. Actual Harmony hook installation, Unity deferred destruction, game event ordering, font/layout, cursor/focus and loader compatibility tests were not run.
- Duplicate plugin initialization is gated; context changes include local user, HUD, body and pause controller. Faults retire additions and suppress repeated reopen within the same visible scoreboard session.
- Exact coverage is in SourceCoverage.md. Four quantitative component IDs are supported, two buff effect descriptions are explicit, other formulas/timers remain unavailable. Sale Star is a reported-issue label only, with no adjusted chance promise.
- ManualSmokeChecklist.md describes later isolated-profile selection, tests and recovery; no steps were performed. The current installed profile still has the old build.
- No software installation, game launch, staging, profile/bootstrap change, save/account mutation, game implementation inspection, asset extraction, new preview upload or publication occurred.

Build command:

    .\build.ps1 -Mode PluginReview -LoaderCorePath '.references\BepInExPack-5.4.2122'

The review directory also records exact hashes of this source-built artifact and the unchanged earlier project/profile DLL. No broad all-item mechanics research is required to assess this milestone.
