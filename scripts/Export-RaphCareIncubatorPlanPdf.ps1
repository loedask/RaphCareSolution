# Regenerates the founder-facing incubator plan PDF under docs/incubator/.
# Markdown and config live under docs/incubator/sources/.
# Requires Node.js (npx) and network on first run for md-to-pdf.
param(
    [string] $Stem = "raphcare-incubator-plan",
    [string] $RepoRoot = ""
)

$ErrorActionPreference = "Stop"

if (-not $RepoRoot) {
    $RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
}

$Stem = $Stem.Trim()
if ($Stem -match '\.(md|pdf|config\.json)$') {
    throw "Pass the document stem only (example: raphcare-incubator-plan). Got: $Stem"
}

$docsRoot = Join-Path $RepoRoot "docs\incubator"
$sourcesRoot = Join-Path $docsRoot "sources"
$md = Join-Path $sourcesRoot "$Stem.md"
$config = Join-Path $sourcesRoot "$Stem.config.json"
$pdf = Join-Path $docsRoot "$Stem.pdf"
$tempMd = Join-Path $sourcesRoot "$Stem.__export__.md"
$tempPdf = Join-Path $sourcesRoot "$Stem.__export__.pdf"

if (-not (Test-Path $md)) {
    throw "Incubator plan markdown not found: $md"
}
if (-not (Test-Path $config)) {
    throw "PDF config not found: $config"
}

$logoPath = Join-Path $docsRoot "brand\raphcare-logo.png"
if (-not (Test-Path $logoPath)) {
    throw "Logo not found: $logoPath"
}

# Chromium blocks ../ and absolute file:// images under md-to-pdf. Copy into the export
# folder and point at a same-directory relative path.
$exportBrandDir = Join-Path $sourcesRoot "brand"
New-Item -ItemType Directory -Force -Path $exportBrandDir | Out-Null
Copy-Item -Path $logoPath -Destination (Join-Path $exportBrandDir "raphcare-logo.png") -Force
$content = Get-Content -LiteralPath $md -Raw -Encoding utf8
$exportContent = [regex]::Replace($content, 'src="[^"]*raphcare-logo\.png"', 'src="brand/raphcare-logo.png"')
$utf8NoBom = New-Object System.Text.UTF8Encoding $false
[System.IO.File]::WriteAllText($tempMd, $exportContent, $utf8NoBom)

Push-Location $sourcesRoot
try {
    npx --yes md-to-pdf "$Stem.__export__.md" --config-file "$Stem.config.json"
    if ($LASTEXITCODE -ne 0) {
        throw "md-to-pdf failed for $Stem (exit $LASTEXITCODE)"
    }
    if (-not (Test-Path $tempPdf)) {
        throw "PDF was not created at $tempPdf"
    }

    try {
        Move-Item -Path $tempPdf -Destination $pdf -Force
    }
    catch {
        $fallback = Join-Path $docsRoot "$Stem.pdf.new"
        Move-Item -Path $tempPdf -Destination $fallback -Force
        throw "Could not overwrite $pdf (file may be open). Fresh PDF saved as $fallback."
    }

    Write-Host "Wrote $pdf"
}
finally {
    Pop-Location
    Remove-Item -Path $tempMd -Force -ErrorAction SilentlyContinue
}
