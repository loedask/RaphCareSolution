# Regenerates a mobile-update PDF.
# Markdown and config live under docs/mobile-updates/sources/.
# Output PDF is written next to the partner-facing PDFs (folder root, or archives/).
# Version must match the APK label: display + Android versionCode, e.g. 1.5.2+10
# Requires Node.js (npx) and network on first run for md-to-pdf.
param(
    [Parameter(Mandatory = $true)]
    [string] $Version,
    [switch] $Archive,
    [string] $RepoRoot = ""
)

$ErrorActionPreference = "Stop"

if (-not $RepoRoot) {
    $RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
}

$Version = $Version.Trim()
if ($Version -notmatch '^\d+\.\d+\.\d+\+\d+$') {
    throw "Version must look like 1.5.2+10 (display version + Android versionCode). Got: $Version"
}

$docsRoot = Join-Path $RepoRoot "docs\mobile-updates"
$sourcesRoot = Join-Path $docsRoot "sources"
$sourceDir = if ($Archive) { Join-Path $sourcesRoot "archives" } else { $sourcesRoot }
$pdfDir = if ($Archive) { Join-Path $docsRoot "archives" } else { $docsRoot }

$stem = "mobile-update-v$Version"
$md = Join-Path $sourceDir "$stem.md"
$config = Join-Path $sourceDir "$stem.config.json"
$pdf = Join-Path $pdfDir "$stem.pdf"
$tempMd = Join-Path $sourceDir "$stem.__export__.md"
$tempPdf = Join-Path $sourceDir "$stem.__export__.pdf"

if (-not (Test-Path $md)) {
    $hint = if ($Archive) { "" } else { " If the note was archived, pass -Archive." }
    throw "Mobile update markdown not found: $md.$hint"
}
if (-not (Test-Path $config)) {
    throw "PDF config not found: $config"
}

$logoPath = Join-Path $docsRoot "brand\raphcare-logo.png"
if (-not (Test-Path $logoPath)) {
    throw "Logo not found: $logoPath"
}

New-Item -ItemType Directory -Force -Path $pdfDir | Out-Null

# Chromium blocks ../ and absolute file:// images under md-to-pdf. Copy into the export
# folder and point at a same-directory relative path.
$exportBrandDir = Join-Path $sourceDir "brand"
New-Item -ItemType Directory -Force -Path $exportBrandDir | Out-Null
Copy-Item -Path $logoPath -Destination (Join-Path $exportBrandDir "raphcare-logo.png") -Force
$content = Get-Content -LiteralPath $md -Raw -Encoding utf8
$exportContent = [regex]::Replace($content, 'src="[^"]*raphcare-logo\.png"', 'src="brand/raphcare-logo.png"')
$utf8NoBom = New-Object System.Text.UTF8Encoding $false
[System.IO.File]::WriteAllText($tempMd, $exportContent, $utf8NoBom)

Push-Location $sourceDir
try {
    npx --yes md-to-pdf "$stem.__export__.md" --config-file "$stem.config.json"
    if ($LASTEXITCODE -ne 0) {
        throw "md-to-pdf failed for $stem (exit $LASTEXITCODE)"
    }
    if (-not (Test-Path $tempPdf)) {
        throw "PDF was not created at $tempPdf"
    }

    try {
        Move-Item -Path $tempPdf -Destination $pdf -Force
    }
    catch {
        $fallback = Join-Path $pdfDir "$stem.pdf.new"
        Move-Item -Path $tempPdf -Destination $fallback -Force
        throw "Could not overwrite $pdf (file may be open). Fresh PDF saved as $fallback."
    }

    Write-Host "Wrote $pdf"
}
finally {
    Pop-Location
    Remove-Item -Path $tempMd -Force -ErrorAction SilentlyContinue
}
