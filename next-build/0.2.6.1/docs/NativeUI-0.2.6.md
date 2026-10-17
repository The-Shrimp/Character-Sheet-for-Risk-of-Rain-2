# Native UI 0.2.6 - focused review and acceptance

## Actual wiring

| Feature | Source connection | Remaining validation |
| --- | --- | --- |
| HUD buff-icon hover | SheetView -> local HUD.buffDisplay -> active BuffIcon -> owned transparent child Canvas/GraphicRaycaster/Image -> pointer handler -> same BuffGlossaryPresentation rows -> NativeHoverPopup | Unity pointer/raycast dispatch, native cursor visibility and actual above-healthbar placement |
| Buff scope and uncertainty | First local HUD viewer + current target body + declared buff display + visible positive native/body stacks; metadata-based Nectar eligible/not counted/unknown; unmatched identities unknown | Live ready/cooldown transitions and pooled icon replacement |
| Actual native inventory expansion | SheetView -> own identity-matched ScoreboardStrip.itemInventoryDisplay -> NativeInventoryViewport wraps that same display; maxIconWidth reduced to at most 36 x scale; four-row minimum footprint; native UpdateDisplay/layout methods | Verify vanilla layout uses these public controls as expected; fit at actual resolution |
| Native inventory scroll | That native display is ScrollRect.content inside RectMask2D; vertical wheel scrolling plus a scrollbar; native preferredHeight determines content height | Long-run icon visibility, wheel/drag behavior, clipping, scroll position after pickups |
| Cleanup | Field ownership leases, original parent/sibling/RectTransform capture, cleanup on Tab close/focus/body/HUD/scene replacement; foreign parent/field changes are preserved; native children escape before owned wrapper destruction | Close/reopen, death/respawn, stage/menu/focus transitions and compatibility with other UI writers |

No new text rows substitute for these features. No copied native inventory or icon prefab is created. Existing native description/current-next item tooltips and partial stat attribution are inherited unchanged. Sorting/search/filter controls remain unwired. This build adds no new item/stat formulas.

## Public API evidence and original code

The installed API differs from older mod source: BuffDisplay has public SetSource but no public source getter. This implementation uses public HUD.buffDisplay and HUD.targetBodyObject instead. Public HUD.localUserViewer and MPEventSystem.localUser/isCursorVisible/cursorOpenerCount gate the local existing cursor context. It writes no cursor count, cursor state, key binding or aim/input state.

The public ItemInventoryDisplay contract supplies maxIconWidth, maxHeight, GetTotalVisibleItemCount, UpdateDisplay and layout input methods/preferredHeight. Only the two local presentation limits are leased; SetItems, SetSubscribedInventory, itemOrder and itemStacks are never written/called by this viewport. Original transform geometry is restored while the parent is owned; distinguishable later anchor/pivot/scale/rotation changes are preserved. Position and size remain driven by the owned scroll/native layout during attachment and return to their captured values.

[Public member signature listing](../artifacts/public-ui-signatures.txt) was produced with ReflectionOnlyLoad; no method body inspection, game code invocation, or asset extraction. [Original listing harness](../artifacts/PublicUiApi.cs).

Public examples reviewed for API names, without copying their implementation:

- [LookingGlass BuffDescriptions, pinned 4aa6dff](https://github.com/Wet-Boys/LookingGlass/blob/4aa6dff73e9df72a50a67b9d9cf449bd6dcaf9cf/LookingGlass/BuffDescriptions/BuffDescriptions.cs): BuffIcon.buffDef and per-icon tooltip API; LGPL-3.0.
- [GrowthNectarDisplay original buff indicator source](https://github.com/Goorakh/RoR2_GrowthNectarDisplay/blob/master/GrowthNectarDisplay/BuffIconIndicator/GrowthNectarBuffIndicatorController.cs): older BuffDisplay source usage and per-instance icon identity; MIT. Reviewed 2026-10-05; moving master, not a guarantee for installed build.
- [LookingGlass AutoSortItems, pinned 4aa6dff](https://github.com/Wet-Boys/LookingGlass/blob/4aa6dff73e9df72a50a67b9d9cf449bd6dcaf9cf/LookingGlass/AutoSortItems/AutoSortItemsClass.cs): public UpdateDisplay usage. Its itemOrder/itemStacks manipulation is not used here.
- [Unity ScrollRect](https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/script-ScrollRect.html) and [RectMask2D](https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/script-RectMask2D.html): content/viewport/mask/scrollbar contracts.

Glossary effects retain the existing pinned source and uncertainty semantics from [0.2.5 glossary coverage](BuffGlossary-0.2.5.md). No exact remaining duration or multiplayer countdown is inferred.

## Focused manual acceptance - after separate staging authorization

The installed 0.2.2 candidate does not contain these two new native features. First establish its native Tab/cursor/pause baseline as planned. This 0.2.6 DLL remains unstaged.

1. In 0.2.6, open the mapped native Info/Tab action, use the game's existing UI cursor, and hover an actual buff icon above HP. Check the name, current stack/state distinction, effect coverage, Nectar classification and duration unavailable label. Shift should reveal source/metadata details. Outside Tab or without its cursor, buff targets must not intercept aim.
2. Test a ready -> cooldown -> ready pair, stacked buff, counter and unfamiliar/modded visible buff. Expired/reused icons must not retain an old entry; hidden buffs must not acquire a target. Confirm spectated/remote HUDs do not bind.
3. Inspect the own native item grid: smaller icons, expanded viewport, native ordering and readable stack counts. With enough unique items, wheel and drag the scrollbar to reach all rows. Confirm item hovers follow the actual native icons and other players' grids are unchanged.
4. Close/reopen Tab and change resolution. Check restoration and renewed native layout. Pickup an item while Tab is visible if multiplayer permits; new icons should join native layout without changing contents/order. Empty/short inventory should remain usable.
5. Test focus loss, pause menu, death/respawn, stage change, run exit and disabling the plugin. Confirm no orphan hit targets, native inventory parent/layout returns and original cursor/pause behavior is retained. A later distinguishable foreign layout writer should cause this viewport to release rather than fight it.

## Validation boundary

Plugin compilation and synthetic ownership/layout/lifecycle scenarios pass. No Unity/game runtime test, deployment or launch occurred. Tests cover local vs remote scope, expired/pool-reused icon ownership, Tab/cursor gating, long/narrow inventory geometry, original field recovery after content growth, foreign field/parent changes, repeated cleanup and respawn/focus session transitions. These checks do not simulate engine raycasts, native layout internals, network timing or third-party mod dispatch.

[Machine-readable verification](../artifacts/next-build-verification.json) records candidate hash, protected/source/reference hashes, and identical-input build comparison. A source-only 0.2.5 freeze was created before editing 0.2.6. Source/tool work needs the computer and connected execution session available; neither the game nor r2modman must remain open. No exact wake or connection guarantee is made.

Final candidate: 166400 bytes; SHA256 FDB12A2870582490A059A1F5D230E41AACFB495F8AF5D462E9A63A477F803483. Identical-input repeated compilation matches. Installed 0.2.2 and all frozen 0.2.3/0.2.4/0.2.5 source and candidate hashes match. 36 mapped root files, 145 external references, and 7 protected config/loader/bootstrap/license files match.
