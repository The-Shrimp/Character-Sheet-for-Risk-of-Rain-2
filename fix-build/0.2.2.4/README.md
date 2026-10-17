# Alexander's Character Sheet — native-tooltip Details candidate 0.2.2.4

Source-only successor to corrected 0.2.2.3; not installed or runtime-tested.

Native item tooltip entry always runs and native title/description/localization/formatting remain intact. An owned clickable Details control adds stack/formula/next-copy/context information below it. Stat/buff details use the same click toggle. No modifier-key expansion or Steam-setting change.

[Architecture, checks and runtime limits](docs/NativeTooltipDetails-0.2.2.4.md).

Build: build.ps1 -Mode CoreTests; then build.ps1 -Mode PluginReview -LoaderCorePath followed by the reviewed loader reference directory. Builds do not deploy or launch the game.

8,681 assertions pass. DLL: 141,312 bytes. SHA256: 08A4E34C4F0AF33DC40505077606BDA619A28B9E450CA6CDCF25433B5FD788DD.

Installed 0.2.2.2 and predecessor/next-feature candidates remain unchanged. Native pointer transfer, tooltip movement/draw order and multiplayer gameplay click behavior require runtime review.
