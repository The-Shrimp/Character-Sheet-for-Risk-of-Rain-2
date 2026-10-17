param(
    [ValidateSet('CoreTests','PresentationCore','GameApi','Plugin','PluginReview')][string]$Mode = 'CoreTests',
    [string]$GameManagedPath = 'C:\Program Files (x86)\Steam\steamapps\common\Risk of Rain 2\Risk of Rain 2_Data\Managed',
    [string]$LoaderCorePath,
    [string]$CompilerPath = 'C:\Program Files (x86)\Microsoft Visual Studio\18\BuildTools\MSBuild\Current\Bin\Roslyn\csc.exe'
)
$ErrorActionPreference = 'Stop'
$projectRoot = $PSScriptRoot
if (!(Test-Path -LiteralPath $CompilerPath -PathType Leaf)) { throw 'Pass -CompilerPath for an installed Roslyn csc.exe. No SDK installation is required by this script.' }
$outputDirectory = if ($Mode -eq 'PluginReview') { Join-Path $projectRoot 'artifacts\review-native-tab' } else { Join-Path $projectRoot 'artifacts' }
New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
$coreSources = Get-ChildItem -LiteralPath (Join-Path $projectRoot 'src\Core') -Filter '*.cs' -File
if ($Mode -eq 'CoreTests') {
    $outputPath = Join-Path $outputDirectory 'CoreTests.exe'
    $testSources = Get-ChildItem -LiteralPath (Join-Path $projectRoot 'tests') -Filter '*.cs' -File
    & $CompilerPath /nologo /deterministic+ /target:exe ('/out:' + $outputPath) $coreSources.FullName $testSources.FullName
    if ($LASTEXITCODE -ne 0) { throw 'Core test compilation failed.' }
    & $outputPath
    if ($LASTEXITCODE -ne 0) { throw 'Core tests failed.' }
    exit 0
}
if ($Mode -eq 'PresentationCore') {
    $presentationNames = @('NumberFormatting.cs','ComparisonPresentation.cs','InventoryPresentation.cs','TooltipPresentation.cs','ItemTooltipPresentation.cs')
    $presentationSources = $presentationNames | ForEach-Object { Join-Path (Join-Path $projectRoot 'src\Core') $_ }
    $outputPath = Join-Path $outputDirectory 'Alexander.PresentationCore.dll'
    & $CompilerPath /nologo /deterministic+ /target:library /optimize+ ('/out:' + $outputPath) $presentationSources
    if ($LASTEXITCODE -ne 0) { throw 'Independent presentation compilation failed.' }
    Write-Output 'Compiled Alexander.PresentationCore.dll using only original presentation source and standard framework references. No game files were read or deployed.'
    exit 0
}
if (!(Test-Path -LiteralPath (Join-Path $GameManagedPath 'RoR2.dll'))) { throw 'Pass the installed game Managed directory.' }
$references = Get-ChildItem -LiteralPath $GameManagedPath -Filter '*.dll' -File | ForEach-Object { '/reference:' + $_.FullName }
$sources = @($coreSources.FullName) + @(Get-ChildItem -LiteralPath (Join-Path $projectRoot 'src\Presentation') -Filter '*.cs' -File | ForEach-Object FullName)
if ($Mode -in @('Plugin','PluginReview')) {
    if ([string]::IsNullOrWhiteSpace($LoaderCorePath)) { throw 'Pass -LoaderCorePath for reviewed BepInEx.dll and 0Harmony.dll; no automatic setup is performed.' }
    foreach ($referenceName in @('BepInEx.dll','0Harmony.dll')) {
        $referencePath = Join-Path $LoaderCorePath $referenceName
        if (!(Test-Path -LiteralPath $referencePath -PathType Leaf)) { throw "Missing $referenceName. Supply reviewed build-only loader references using -LoaderCorePath; this script never downloads or installs dependencies." }
        $references += '/reference:' + $referencePath
    }
    $sources += Join-Path $projectRoot 'src\Plugin.cs'
}
$outputName = if ($Mode -in @('Plugin','PluginReview')) { 'Alexander.CharacterSheet.dll' } else { 'GameUI.ApiCheck.dll' }
& $CompilerPath /nologo /deterministic+ /noconfig /nostdlib+ /target:library /optimize+ ('/out:' + (Join-Path $outputDirectory $outputName)) $references $sources
if ($LASTEXITCODE -ne 0) { throw 'Compilation failed.' }
Write-Output "Compiled $outputName. No files were deployed and no game was launched."

