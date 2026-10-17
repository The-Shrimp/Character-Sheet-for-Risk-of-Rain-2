# Native item tooltip plus clickable Details — 0.2.2.4

Source-only successor to the corrected 0.2.2.3 candidate. User direction on 2026-10-06 supersedes replacement item tooltips and modifier-key expansion. Installed 0.2.2.2 remains unchanged. No deployment, game launch/close/input, profile write, Steam-setting change, software installation, publishing or proprietary implementation inspection occurred.

## Item architecture

The native TooltipProvider pointer-enter implementation always runs. The add-on observes the completed native event and associates an existing active TooltipController only when its public owner.currentTooltipProvider is the exact tracked own provider. Zero or multiple matches decline enhancement. Native title, item description, localization, semantic formatting and ordinary tooltip content are never passed through the add-on renderer or written by this code.

An owned sidecar below the original tooltip starts with a clickable Details control and clover badge. Expanding shows current effective-stack component results, concise additional-copy formulas, next-copy values/benefits and existing model context limits. It adds calculations without replacing or duplicating the native item description/title.

The sidecar remains inside NativeSheetLayer below the existing scoreboard HUD branch. No direct HUD/root-canvas child or native parent/geometry changes are made. It pins the first observed native tooltip bounds to keep its control stationary during pointer transfer. It does not freeze or move the native tooltip. If no safe space remains beneath the native tooltip, augmentation is declined. Expanded content has a bounded scrolling viewport.

For the existing owned inventory/effect rows, a provider created on that owned row copies the public titleText/bodyText and colors from a matched own native icon. The original native provider stays read only. If no native reference is available, that owned-row provider stays disabled. This is part of the existing added row presentation; no native inventory scroll/resize or HUD-buff feature branch was imported.

## Pointer transfer and controls

A 0.4-second unscaled transfer window lets the pointer move from an own item into its controls. Only a matched own provider with a visible add-on can defer its actual native exit. The lease keeps the native tooltip accessible while the pointer remains on the add-on and replays the recorded public Unity exit interface once when leaving/closing.

Pending exits are released before another provider enters the same actual event-system context. Reentering the same source cancels its pending exit. If the native context is replaced, cleanup discards the stale lease rather than hiding another provider's tooltip. Provider disable, Tab visibility loss, source change, focus/scene/session cleanup and destruction release owned state. Recycled item identity requires a fresh hover; it cannot keep a previous item's expanded state.

Details uses an owned HGButton and native cursor visibility/opener checks. Its pointer down/up/click and submit handlers consume their UI events; the owned background also consumes clicks. No gameplay input is fabricated or globally rewritten. Gameplay click suppression in multiplayer still relies on native Tab cursor routing and requires runtime verification.

There is no modifier-key handler, chord, Steam setting change or shortcut hint. Stat and added buff details use the same clickable toggle. Compact base-stat structure remains total/base/nonzero growth/applicable sources, with an unattributed remainder only when nonzero. The regression health case remains total209, base110, growth+99.

## Validation

- 8,681 assertions pass; 5,924 added to the predecessor's 2,757.
- New tests cover open/close clicks, invalid cursor/secondary clicks, pointer transfer, repeated callbacks/item changes, recycled identities, close/reopen and repeated cleanup.
- 24 Tab sessions x 16 item changes, with 12 transfer frames per item; scoped native exits release at most once and clear even on callback failure.
- 36 additive placement cases plus the inherited 96 tooltip sizing cases; no room/unknown geometry fails closed.
- Inherited mathematical, lifecycle, pause, ownership, formula and screenshot regressions remain unchanged and pass.
- Current-stack/additional-copy/next-copy values and missing-context honesty tested.
- Public native owner/current-provider, content, cursor, button and pointer interfaces compile against installed read-only references. Probe assemblies were not executed.
- Source audit finds no native title/body or current-provider writes, no modifier handler/hint, and no native-enter suppression.
- Plugin.cs differs from 0.2.2.3 only in its two version strings. Stat math, formula definitions, pause/session guard, native sheet host and docking geometry preserved.
- Parent 0.2.2.3 DLL/source hashes match the saved corrected review record. Both separate next-feature DLLs remain unchanged.

DLL: 141,312 bytes. SHA256: 08A4E34C4F0AF33DC40505077606BDA619A28B9E450CA6CDCF25433B5FD788DD.
Lineage parent SHA256: 49F6A0B90ADF8D341CD457CCB0C7719B9273ABE904D8A22B996B726E03A6D915.
Installed 0.2.2.2 SHA256: 6C54A2FACA7A9751DD240EF3514647347C4B2BD86660378177C11F662964A396; exactly one loadable custom DLL.
Nine supporting files match their recorded baseline, including the own configuration's previously explained header-only update. No profile writes were issued.

Evidence: artifacts/ADDITIVE-DETAILS-VERIFICATION.json and artifacts/ADDITIVE-DETAILS.diff.

## Runtime review still required

No live testing occurred. Before authorized deployment, review this isolated candidate. Then verify that the ordinary native item title/description/localization/formatting remain intact, Details stays reachable while transferring the pointer, toggle/scroll/clover placement is readable, and switching items or closing Tab never leaves a native tooltip stuck.

Check native movement/draw order and screen edges. Sidecar pins its initial native bounds while the native tooltip retains its ordinary movement; exact visual composition is unverified. Check own added-row copies with existing native content/other mods and verify unavailable references decline cleanly.

Verify mouse/gameplay click handling in single player and multiplayer, pause/resume, native hold/release, focus/Steam overlay and cleanup. No claim of verified runtime behavior is made.

Public interaction references: [Unity pointer exit contract](https://docs.unity3d.com/2019.1/Documentation/ScriptReference/EventSystems.IPointerExitHandler.html), [UI event consumption](https://docs.unity3d.com/2019.1/Documentation/ScriptReference/EventSystems.AbstractEventData.Use.html). Native APIs were validated by compilation, without implementation inspection.
