# Alexander's Character Sheet - isolated 0.2.6 review

Compiled source candidate only. Installed 0.2.2 remains the first test candidate; 0.2.5 is frozen. No automatic deployment or launch.

This pass wires two requested native features:

- Hover targets on the local HUD's actual BuffIcon instances above the health display, using the existing 19-state glossary, live stack observations, explicit unknown effects, metadata-based Nectar eligibility and unavailable remaining durations. Targets require the existing local Tab UI cursor context. Shared prefabs, icon images and native provider data are untouched.
- A ScrollRect/RectMask2D viewport around the actual local scoreboard ItemInventoryDisplay, with a smaller public maxIconWidth, native layout refresh, a four-row minimum footprint, vertical wheel scrolling and scrollbar. The original parent, sibling, layout geometry and owned field values are restored on cleanup. Inventory contents, stack arrays and item order are not written.

These are source-wired and compile successfully. Unity raycast dispatch, actual inventory reflow/scrolling, visual fit and restoration still need in-game testing. Synthetic checks do not establish runtime success.

[Focused acceptance and evidence](docs/NativeUI-0.2.6.md). Prior versioned readiness documents are historical evidence for their named versions. Inherited 0.2.5 item/stat math coverage remains partial and unchanged.

Build only with the existing local compiler and external references:

    .\build.ps1 -Mode CoreTests
    .\build.ps1 -Mode PluginReview -LoaderCorePath '<project>\.references\BepInExPack-5.4.2122'

No dependency download, software install, game/profile/bootstrap/configuration or power/security change, publication, or game implementation inspection occurs.
