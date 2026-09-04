# Regenerates a mobile-update PDF from docs/mobile-updates/mobile-update-v{Version}.md
# Requires Node.js (npx) and network on first run for md-to-pdf.
param(
    [Parameter(Mandatory = $true)]
    [string] $Version,
    [string] $RepoRoot = ""
)

$ErrorActionPreference = "Stop"

if (-not $RepoRoot) {
    $RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
}

$docs = Join-Path $RepoRoot "docs\mobile-updates"
$stem = "mobile-update-v$Version"
$md = Join-Path $docs "$stem.md"
$config = Join-Path $docs "$stem.config.json"
$pdf = Join-Path $docs "$stem.pdf"
$tempMd = Join-Path $docs "$stem.__export__.md"
$tempPdf = Join-Path $docs "$stem.__export__.pdf"

if (-not (Test-Path $md)) {
    throw "Mobile update markdown not found: $md"
}
if (-not (Test-Path $config)) {
    throw "PDF config not found: $config"
}

Copy-Item -Path $md -Destination $tempMd -Force

Push-Location $docs
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
        $fallback = Join-Path $docs "$stem.pdf.new"
        Move-Item -Path $tempPdf -Destination $fallback -Force
        throw "Could not overwrite $pdf (file may be open). Fresh PDF saved as $fallback."
    }

    Write-Host "Wrote $pdf"
}
finally {
    Pop-Location
    Remove-Item -Path $tempMd -Force -ErrorAction SilentlyContinue
}
