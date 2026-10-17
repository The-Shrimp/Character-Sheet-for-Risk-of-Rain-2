# Independent presentation core

This milestone is original source and synthetic tests. It does not reference Unity/RoR2, inspect proprietary game files, validate game mechanics, or run the game. It is not integrated into the existing game adapter/staged plugin.

## Modules

| Module | Supplied input | Output / rule |
|---|---|---|
| NumberFormatting | Nullable numbers/counts | Compact rows, round-trip detailed values, signed changes, explicit Unavailable |
| ComparisonPresentation | Supplied current/next values, units, basis, evidence and conditions | Component/estimate labels; arithmetic only when both values are finite and comparable |
| AttributionPresentation | Supplied live/model values and completeness metadata | Live total retained; discrepancies and incomplete explanations disclosed |
| InventoryPresentation | Immutable own-row snapshots and filter choices | Separate deterministic visible list; no source order/count mutation |
| TooltipPresentation | Plain text, comparison output and detail sections | Short/full strings; uncertainty retained; bounded text and explicit truncation |
| ItemTooltipPresentation | Caller-supplied base, increment, total copies, evidence and luck metadata | Inline scaling headline; exact additional-copy and next-copy math; no formula discovery or verification |

The core labels a numerical increase/decrease/no immediate change, rather than guessing whether higher is better or ranking an item's value. It supplies no new item/buff formulas. Unverified next-copy values cannot be rendered as zero benefit. Component terms cannot be compared to live totals. A matching subtotal does not complete attribution unless the source explicitly marks it complete.

Attribution tolerance defaults to exact comparison. A future adapter can explicitly provide a finite nonnegative tolerance; millions do not automatically permit a large missing source.

## Large and small numbers

| Supplied value | Row | Full precision |
|---|---|---|
| 12,500,000.125 | 12.5M | 12,500,000.125 |
| -1,250,000 | -1.25M | -1,250,000 |
| 0.00005 | 5e-5 | Round-trip supplied value |
| Missing / NaN / Infinity | Unavailable | Unavailable |

Compact values are rounded. Full values use the supplied double's round-trip representation, not an invented higher-precision measurement. Extreme magnitudes use scientific notation to remain bounded. A small +0.5 change beside millions survives in detailed comparison text. Stack counts remain integer values, including the full range of long, rather than being converted through double. Formatting is invariant across the process's culture.

## Synthetic adapter example

    var comparison = ComparisonPresentation.Create(new ComparisonInput {
        Current = 12500000.125,
        Next = 12500000.625,
        Unit = ValueUnit.Health,
        Evidence = PredictionEvidence.ConditionalEstimate,
        CurrentBasis = ComparisonBasis.Total,
        NextBasis = ComparisonBasis.Total,
        Conditions = "same supplied source state"
    });
    var tooltip = TooltipContent.FromComparison(
        "Synthetic row", "Provided values only", comparison);
    string shortText = TooltipPresentation.Select(tooltip, false);
    string detailedText = TooltipPresentation.Select(tooltip, true);

The input's evidence/basis/conditions must be supplied correctly by a reviewed adapter. The core cannot establish that a game formula is correct. An unsupported prediction must use PredictionEvidence.Unavailable even if some placeholder number exists.

## Inventory behavior

- Rarity/name default, name, effective-count, acquisition and effect-group sorts are deterministic; stable IDs break name/case ties.
- Search requires all words, ignoring case; it can match names, supplied group labels and additional supplied search text.
- Group and luck filters retain mixed classifications. Unknown luck is Unverified, never automatically Unaffected.
- Hidden or confirmed-absent entries are excluded. Unknown counts remain visible with Unavailable labels; active-only filters require a confirmed positive effective count.
- Owned but inactive entries show both counts and an inactive label.
- Rows and group metadata are copied into immutable snapshots/read-only results. Duplicate IDs are rejected so selection cannot point at the wrong row.
- Selection survives sort changes by ID. Empty results have no fabricated selected row.

These are presentation policies. A future adapter must supply only authorized own-character rows and metadata. This core cannot verify ownership or information permissions itself.

## Tooltip behavior and remaining visual work

Strings are plain text. A renderer must disable rich-text interpretation for these strings; the core deliberately does not execute or interpret supplied markup. Control characters are cleaned, intentional detailed lines retained, and collapsed labels normalize whitespace.

Short text defaults to420 characters and reserves space for confidence/uncertainty. Full text defaults to8192 characters, puts uncertainty near the top and labels truncation. Character clipping does not split a surrogate pair, including in a custom truncation marker. These are character bounds, not pixel-layout guarantees.

The actual renderer still needs reviewed adapter integration, readable fonts/scale, scrolling for long detail, keyboard/controller focus decisions, and real screen tests. A labelled standalone design image has been rendered and visually checked; no game UI validation is claimed.

## Checks and gates

Permitted independent build/test modes during the hold:

    .\build.ps1 -Mode CoreTests
    .\build.ps1 -Mode PresentationCore

CoreTests compiles src/Core and tests before exiting. PresentationCore compiles only the five original presentation modules to artifacts/Alexander.PresentationCore.dll using the installed compiler's standard framework references. Both modes exit before inspecting the game's Managed directory. The standalone library is an original-code reuse/build artifact, not a game plugin or runtime-compatibility certification. The independent presentation suite exercises million/tiny/nonfinite values, exact detail, mixed/unknown metadata, incompatible comparisons, attribution discrepancy,2,000 rows, deterministic sorting and bounded Unicode tooltips.

Do not run Plugin/GameApi builds, stage a new DLL, launch a game, alter installed profiles/bootstrap or extract further game content while the parent/user terms/account hold remains. Native game screenshot/input tooling also remains unavailable. Existing staged plugin and installed pieces stay unchanged. The separate action inventory describes the earlier setup.

## Verification checkpoint

- Standalone synthetic suite: PASS193 assertions, including152 independent presentation/native-policy assertions and41 existing core assertions.
- New source/document whitespace checks pass; the five presentation modules have no Unity/RoR2 namespace references.
- Prior plugin artifact remains SHA256 D70F964C01EC5D929A3AA29F14D77EB9B0CC3F7A63F9C93325D2BDA5B16E208C.
- No Plugin/GameApi compilation, DLL staging, installed/profile/bootstrap changes, game launch or further game-content inspection occurred during this milestone.


The supplied Prayer Beads design example is covered by24 item-tooltip assertions, including the total-count minus-one rule, zero/first/unknown copies, next marginal change, count/numeric overflow, nonfinite values and unknown/mixed luck. The renderer consumes the original model's headline/full math through a UTF-8 JSON export. Its existing-browser render is synthetic data only; no further game content was read. The Library image was created successfully, but the supported metadata helper could not apply local identity xattrs because this Windows Python lacks os.setxattr. This does not undo the successful Library upload.


The later parent-authorized native Tab source integration is documented in NativeTabImplementation.md. Number formatting and item templates are wired into that source adapter; it is locally compiled to an isolated review DLL. The older staged plugin/profile/bootstrap remain untouched. PluginReview compilation was permitted using local assemblies as references without further implementation inspection. Earlier milestone hold wording above describes its original checkpoint; no runtime launch is authorized by the build.
