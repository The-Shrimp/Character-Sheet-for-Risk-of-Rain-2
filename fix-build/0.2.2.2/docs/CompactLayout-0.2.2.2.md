# Compact native layout candidate 0.2.2.2

Source-only candidate derived from installed 0.2.2.1. Not deployed or game-tested.

Parent screenshot observation: 1920x1080 native scoreboard x462..1457, y289..363; old additions x1483..1786, y562..1064, overlapping skill HUD with unused vertical space.

The stats card now anchors below the measured own-player strip, aligned left. It fits the existing eight rows, header and disclosure instead of requesting a 510-unit panel. Labels and live values use separate columns, with right-aligned values. Native runtime fonts/colors remain references; readable body size is derived from visible native text and bounded to 18..24 before presentation scaling. Background sampling prefers the strip root or a panel-sized sliced image, with a thin matching border. There is no dashboard, search or new data feature.

Expanded effects use a scroll viewport capped at 45 percent of the owned canvas height. Native player rows and visible skill/equipment, health, experience and buff UI are occupied geometry. Placement chooses the highest free vertical interval below the own strip; it can shorten the scroll viewport or decline the card, and never chooses a detached HUD corner.

Stable control source from 0.2.2.1 is preserved except version registration/log text: HUD branch ownership, activeSelf session intent, Info fresh-press guard, pointer routing and solo/external pause lease logic are unchanged. No next-feature branch is changed.

Tests model 1280x720, 1366x768, 1920x1080, 2560x1440 and 3440x1440; scales 0.7/0.85/1/1.2; font samples 18/24; collapsed/large effects; stacked native rows, occupied bottom HUD and invalid/unknown geometry. Compilation uses local reference metadata only. No further proprietary implementation inspection.

Runtime unknowns: actual native font/background selection, rect/layout scaling, clipping, pointer/scroll behavior and screenshot composition require a later authorized visual test. User confirmation of 0.2.2.1 pause/resume remains pending.
