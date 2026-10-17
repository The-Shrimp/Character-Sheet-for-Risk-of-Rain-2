Current quality update0.2.1: see Readiness-0.2.1.md, SourceCoverage.md and ManualSmokeChecklist.md. Source lifecycle and local attachment regressions now pass247 assertions. Native provider state suppression/restoration from the earlier checkpoint below was replaced with field-mutation-free public-interface routing; actual Unity/game runtime tests remain unrun.

# Native Tab extension: original source milestone0.2.0

Implemented in source and compiled locally. No game launch or runtime UI/input validation occurred. The earlier staged0.1.0 plugin, isolated mod profile and disabled bootstrap remain unchanged.

## Functional source coverage

| Component | Behavior |
|---|---|
| Plugin | Observes the native scoreboard visibility/mapped Tab action; no forced toggle, key remapping, cursor opener writes or native-panel close writes. Captures pause state before native opening, adopts native pause without duplicate requests, protects borrowed pause and acknowledges owned native close. Cleans up on death, scene/HUD change, focus loss, disable and exceptions. Multiplayer requests no pause. |
| NativeStripIdentity | Uses the public ScoreboardStrip.SetMaster callback to mark only the local master's strip. Does not read a private master field or remote body/inventory. Reassignment clears the identity. |
| SheetView | Leaves native scoreboard/player/inventory objects, transforms, order and counts intact. Adds a scrollable core-stats panel in an available canvas region outside the aggregate native strip bounds. Collapsible grouped own inventory/effects and active-buff glossary; no search/default dashboard tabs. |
| NativeUiStyle | References the existing own-strip TMP font/material/color and a native sliced background sprite at runtime. Creates only original UI objects. Uses a simple fallback background if no native sliced image exists; no extracted/bundled assets or cloned game controllers. |
| NativeItemHover | Attaches only to eligible own visible native ItemIcon objects found through Unity child lookup. Routes the public pointer-enter callback only for an eligible local active exclusive claim. Never writes provider tokens/enabled flags; releases only its own claim on detach. |
| NativeHoverPopup | Plain-text current/next details, Shift/full math with scrolling, bounded positioning and a bottom-right runtime clover sprite with text/symbol. Pointer retention allows entering the popup/luck detail. |
| SheetData | Reads live own CharacterBody stats. Exposes baseline/level growth and known crit term with explicit incomplete attribution. Wires the original ItemTooltipPresentation model for crit/flat-health component terms and conditional Prayer Beads storage scaling. Other item predictions remain unavailable. Buff entries show stacks, flags/counter distinction and Nectar eligibility. |
| NativeTabPolicy / PauseLease | Game-independent ownership, non-overlapping geometry, conditional UI restoration and native pause-adoption/close rules. |

Prayer Beads uses the accepted headline: Stores35% (20 + 5 x each additional item) of level-up stat gains for four effective copies. The next component is40% (+5 percentage points). It is labelled conditional: effective copies are assumed to count; stored levels, cached payout, conversions and state changes are separate. Public formula agreement is not installed-build/runtime verification.

## Public interfaces and source references

Reference-only public source (no third-party implementation copied into src; no new LookingGlass dependency):

- LookingGlass LGPL-3.0, commit4aa6dff73e9df72a50a67b9d9cf449bd6dcaf9cf:
  https://github.com/Wet-Boys/LookingGlass/blob/4aa6dff73e9df72a50a67b9d9cf449bd6dcaf9cf/LookingGlass/ItemStats/ItemStats.cs
  Demonstrates ItemIcon.SetItemIndex, tooltipProvider, itemIndex, ScoreboardStrip.SetMaster and itemInventoryDisplay.
- Public Prayer Beads identifier/component context:
  https://github.com/Wet-Boys/LookingGlass/blob/4aa6dff73e9df72a50a67b9d9cf449bd6dcaf9cf/LookingGlass/ItemStats/ItemDefinitions.cs
- Public runtime TMP usage:
  https://github.com/Wet-Boys/LookingGlass/blob/4aa6dff73e9df72a50a67b9d9cf449bd6dcaf9cf/LookingGlass/StatsDisplay/StatsDisplayClass.cs
- Unity TMP and pointer-event documentation:
  https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.0/api/TMPro.TMP_Text.html
  https://docs.unity3d.com/2019.1/Documentation/ScriptReference/EventSystems.IPointerEnterHandler.html
- Moffein ItemStats MIT public interface reference:
  https://github.com/Moffein/ItemStats/blob/97dd3c912bedd78347afca92ef1b5400cc71956f/ItemStats/ItemStatsPlugin.cs

Compiler checks showed that ScoreboardStrip.master and ItemInventoryDisplay.itemIcons from the public examples are not accessible in the installed reference assemblies. The implementation instead uses the public SetMaster argument and GetComponentsInChildren<ItemIcon>. It does not publicize, reflect private native members, decompile or inspect game implementations.

Native style resources are referenced only when the future plugin runs. Local compilation uses the installed game assemblies as references; their contents were not transferred into model context or uploaded.

## Verification and review artifact

    .\build.ps1 -Mode CoreTests
    .\build.ps1 -Mode PluginReview -LoaderCorePath '.references\BepInExPack-5.4.2122'

The review output is artifacts/review-native-tab/Alexander.CharacterSheet.dll. PluginReview is separate from the old artifacts/ DLL and never deploys it. Synthetic suite:193 assertions (including33 new native-policy/full-attribution assertions). The review compile uses the existing compiler/local references only. No software or dependency installation is performed.

Synthetic coverage includes local/remote/hidden/absent/inactive/temporary ownership, bounded/non-overlapping/missing/crowded geometry, foreign UI changes and repeated restoration, native pause adoption without duplicate calls, controller replacement, external takeover and native close acknowledgement. Detailed attribution retains small changes beside millions and actual multiline text. This establishes source policies and compile-time interfaces, not actual native UI behavior.

## Remaining pieces and runtime gates

- Native screen/input automation remains unavailable. No solo/multiplayer launch, screen capture or account/save test occurred.
- The native Strip.SetMaster hook, TMP font/style choice, available dock geometry, native icon pooling, popup pointer/scroll behavior, pause event ordering and coexistence with other UI mods require runtime validation.
- If native identity/font/canvas is unavailable, the addition waits. If there is no adequately sized free region, the stats panel stays hidden and native panels remain untouched; eligible enhanced item hovers can remain attached. There is no full-screen replacement fallback.
- A public per-stack remaining-buff-timer adapter is still missing. All remaining durations are explicitly unavailable; no private timedBuffs reflection or invented multiplayer countdown is used.
- Full stat-source attribution and most item/buff quantitative formulas are incomplete. Unknown terms and conditional next-copy claims are labelled. Buff names use definition-derived labels where no reviewed localized name mapping exists.
- Controller navigation/focus is unvalidated. Native cursor/key behavior is left to the game.
- The existing core-only loader combination has not been established runtime-compatible. Do not change it, stage this build or launch until the parent/user reviews the compiled milestone.

The earlier Library PNG remains a synthetic temporary layout study. No new preview image was rendered or published during this implementation task. No compliance guarantee or third-party outreach is claimed.
