# Alexander's Character Sheet 0.3.0

Isolated source-only successor to installed 0.2.2.5. Implements nested Base Stats and conditional effect sections, corrected Defense semantics, original margin icons, bounded scrolling, keyboard/focus controls and an 80% character-sheet background. Native item tooltip extensions remain intact.

9928 pure-source assertions pass. Plugin and public API probe compile without warnings. Nothing is deployed; runtime UI verification remains pending. General CDR stays Unknown rather than mixing per-skill values.

Review docs/CharacterSheetHierarchy-0.3.0.md, artifacts/HIERARCHY.diff and artifacts/HIERARCHY-VERIFICATION.json. Use build.ps1 -Mode CoreTests or -Mode PluginReview with the reviewed local loader reference directory. Do not copy artifacts into a profile without later deployment coordination.
