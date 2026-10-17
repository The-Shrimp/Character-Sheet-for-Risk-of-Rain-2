# Alexander's Character Sheet — tooltip candidate 0.2.2.3

Source-only review candidate based on 0.2.2.2. Not installed or runtime-tested. Working Tab layout, lifecycle and pause guard preserved; no 0.2.6.1 features imported.

Concise defaults show current-stack components and short additive stack hints. Shift shows next-copy benefit, exact calculations and evidence. Base-stat hovers show total, base, applicable level growth/source groups and an unattributed remainder when nonzero. Native public tooltip text/style references supply hierarchy and semantic formatting; no replacement color palette.

[Coverage, limitations and tomorrow's review](docs/TooltipPass-0.2.2.3.md).

Build with build.ps1 -Mode CoreTests, then build.ps1 -Mode PluginReview -LoaderCorePath followed by the reviewed loader reference directory. The latter uses local read-only game references and does not deploy or launch anything.

Validation: 2,757 passing assertions. Final review DLL: 131,072 bytes. SHA256: 49F6A0B90ADF8D341CD457CCB0C7719B9273ABE904D8A22B996B726E03A6D915.

Runtime style matching and the identity of any reported missing hooks remain unverified; neither screenshot establishes a broken hover target. Installed 0.2.2.2 remains in place.
