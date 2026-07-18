# Regenerates docs/raphcare-feature-checklist.pdf from the markdown source.
# Requires Node.js (npx) and network on first run for md-to-pdf.
param(
    [string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

$ErrorActionPreference = "Stop"

$md = Join-Path $RepoRoot "docs\raphcare-feature-checklist.md"
$pdf = Join-Path $RepoRoot "docs\raphcare-feature-checklist.pdf"
$config = Join-Path $RepoRoot "docs\raphcare-feature-checklist.pdf.json"

if (-not (Test-Path $md)) {
    throw "Checklist markdown not found: $md"
}
if (-not (Test-Path $config)) {
    throw "PDF config not found: $config"
}

Push-Location (Join-Path $RepoRoot "docs")
try {
    npx --yes md-to-pdf "raphcare-feature-checklist.md" --config-file "raphcare-feature-checklist.pdf.json"

    if (-not (Test-Path $pdf)) {
        throw "PDF was not created at $pdf"
    }

    Write-Host "Wrote $pdf"
}
finally {
    Pop-Location
}
