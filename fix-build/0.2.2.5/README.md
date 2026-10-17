# Alexander's Character Sheet 0.2.2.5

Source-only candidate derived from 0.2.2.4. The ordinary stat tooltip follows Total / Base (base plus coefficient per level) / Bonuses (flat then modifiers). Native item tooltips are retained and extended for both own scoreboard and top HUD inventory paths during native Tab. Unsupported item formulas produce no filler popup.

8812 pure-source assertions pass; the plugin and public inventory API probe compile without warnings. No deployment or runtime validation occurred; the actual host installed version remains 0.2.2.2.

Review docs/NativeTooltipSimpleStats-0.2.2.5.md, artifacts/SIMPLE-STATS-VERIFICATION.json and artifacts/SIMPLE-STATS.diff. Build from this directory with build.ps1 -Mode CoreTests or -Mode PluginReview using the reviewed local loader references. Do not copy artifacts into a profile without separate deployment authorization.
