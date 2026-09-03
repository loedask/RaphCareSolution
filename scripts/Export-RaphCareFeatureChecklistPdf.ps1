# Regenerates feature checklist PDFs from markdown sources under docs/checklist/sources.
# Output PDFs are written to docs/checklist/ (partner-facing).
# Requires Node.js (npx) and network on first run for md-to-pdf.
param(
    [string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path,
    [ValidateSet("engineering", "partner", "all")]
    [string]$Which = "all"
)

$ErrorActionPreference = "Stop"

$docsChecklist = Join-Path $RepoRoot "docs\checklist"
$sources = Join-Path $docsChecklist "sources"

function Export-ChecklistPdf {
    param(
        [Parameter(Mandatory = $true)][string]$BaseName
    )

    $md = Join-Path $sources "$BaseName.md"
    $pdf = Join-Path $docsChecklist "$BaseName.pdf"
    $config = Join-Path $sources "$BaseName.pdf.json"
    $tempMd = Join-Path $sources "$BaseName.__export__.md"
    $tempPdf = Join-Path $sources "$BaseName.__export__.pdf"

    if (-not (Test-Path $md)) {
        throw "Checklist markdown not found: $md"
    }
    if (-not (Test-Path $config)) {
        throw "PDF config not found: $config"
    }

    Copy-Item -Path $md -Destination $tempMd -Force

    Push-Location $sources
    try {
        npx --yes md-to-pdf "$BaseName.__export__.md" --config-file "$BaseName.pdf.json"
        if ($LASTEXITCODE -ne 0) {
            throw "md-to-pdf failed for $BaseName (exit $LASTEXITCODE)"
        }
        if (-not (Test-Path $tempPdf)) {
            throw "PDF was not created at $tempPdf"
        }

        try {
            Move-Item -Path $tempPdf -Destination $pdf -Force
        }
        catch {
            $fallback = Join-Path $docsChecklist "$BaseName.pdf.new"
            Move-Item -Path $tempPdf -Destination $fallback -Force
            throw "Could not overwrite $pdf (file may be open in a viewer). Fresh PDF saved as $fallback. Close the old PDF, replace it with the .pdf.new file, then delete .pdf.new."
        }

        Write-Host "Wrote $pdf"
    }
    finally {
        Pop-Location
        Remove-Item -Path $tempMd -Force -ErrorAction SilentlyContinue
    }
}

$targets = switch ($Which) {
    "engineering" { @("raphcare-feature-checklist") }
    "partner" { @("raphcare-feature-checklist-partner") }
    default { @("raphcare-feature-checklist", "raphcare-feature-checklist-partner") }
}

foreach ($name in $targets) {
    Export-ChecklistPdf -BaseName $name
}
