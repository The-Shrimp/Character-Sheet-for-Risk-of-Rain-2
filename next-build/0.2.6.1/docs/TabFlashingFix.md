# Native Tab flashing correction 0.2.6.1

This candidate derives from 0.2.6. It is not deployed and has no in-game verification.

Evidence: the failed installed 0.2.2 log contains 121 repeated attach statuses without a plugin exception. Read-only native HUD inspection shows HUD.Update activates only its last direct child and HUD.Awake stores the Canvas on the HUD object. Previous stats/popup creation under the root canvas can alter that direct-child list. This is a concrete structural defect consistent with the observed flicker; the exact failing runtime hierarchy was not captured.

Changes: all owned stats/popup objects live inside one existing native scoreboard HUD branch. No native parent, sibling, visibility, input mapping or cursor field is changed. Coordinates use the owned branch-sized layer. Session intent follows the native scoreboard's activeSelf, retaining one view across temporary ancestor invisibility while hiding only our owned layer. If native Info closes and reopens without a fresh mapped press, enhancements are withheld for that hold to prevent pause reacquisition; normal release/new press and native programmatic openings remain available. Existing solo pause lease and external pause ownership are retained.

Regression coverage includes the game's last-child visibility rule, repeated hold/release, popup sibling isolation, ancestor visibility loss/restoration, simulated pause-related Info loss, fresh-press recovery, HUD replacement, and recursive/external pause cleanup. These are modeled tests, not Unity rendering or timing validation.

The baseline hotfix contains no 0.2.6 UI additions. The 0.2.6.1 counterpart inherits the same correction while retaining its prior feature work. Original source and DLL candidates are preserved.
