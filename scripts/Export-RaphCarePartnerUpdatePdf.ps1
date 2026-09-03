# Regenerates a partner-facing PDF under docs/partner-updates/.
# Markdown and config live under docs/partner-updates/sources/.
# Output PDF is written to the folder root (or archives/ when -Archive is set).
# Requires Node.js (npx) and network on first run for md-to-pdf.
param(
    [Parameter(Mandatory = $true)]
    [string] $Stem,
    [switch] $Archive,
    [string] $RepoRoot = ""
)

$ErrorActionPreference = "Stop"

if (-not $RepoRoot) {
    $RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
}

$Stem = $Stem.Trim()
if ($Stem -match '\.(md|pdf|config\.json)$') {
    throw "Pass the document stem only (example: partner-update-2026-09-02 or raphcare-price-list). Got: $Stem"
}

$docsRoot = Join-Path $RepoRoot "docs\partner-updates"
$sourcesRoot = Join-Path $docsRoot "sources"
$sourceDir = if ($Archive) { Join-Path $sourcesRoot "archives" } else { $sourcesRoot }
$pdfDir = if ($Archive) { Join-Path $docsRoot "archives" } else { $docsRoot }

$md = Join-Path $sourceDir "$Stem.md"
$config = Join-Path $sourceDir "$Stem.config.json"
$pdf = Join-Path $pdfDir "$Stem.pdf"
$tempMd = Join-Path $sourceDir "$Stem.__export__.md"
$tempPdf = Join-Path $sourceDir "$Stem.__export__.pdf"

if (-not (Test-Path $md)) {
    $hint = if ($Archive) { "" } else { " If the note was archived, pass -Archive." }
    throw "Partner update markdown not found: $md.$hint"
}
if (-not (Test-Path $config)) {
    throw "PDF config not found: $config"
}

New-Item -ItemType Directory -Force -Path $pdfDir | Out-Null
Copy-Item -Path $md -Destination $tempMd -Force

Push-Location $sourceDir
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
        $fallback = Join-Path $pdfDir "$Stem.pdf.new"
        Move-Item -Path $tempPdf -Destination $fallback -Force
        throw "Could not overwrite $pdf (file may be open). Fresh PDF saved as $fallback."
    }

    Write-Host "Wrote $pdf"
}
finally {
    Pop-Location
    Remove-Item -Path $tempMd -Force -ErrorAction SilentlyContinue
}
