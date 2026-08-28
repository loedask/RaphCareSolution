<#
.SYNOPSIS
  Applies all RaphCare EF Core DbContext migrations to the Azure SQL database from artifacts.
#>
[CmdletBinding()]
param(
    [string] $EnvironmentJsonPath = "",
    [string] $ConnectionString = ""
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
if (-not $EnvironmentJsonPath) {
    $EnvironmentJsonPath = Join-Path $repoRoot "artifacts\azure-test-environment.json"
}

if (-not $ConnectionString) {
    if (-not (Test-Path $EnvironmentJsonPath)) {
        throw "Provide -ConnectionString or run New-RaphCareAzureTestEnvironment.ps1 first."
    }
    $ConnectionString = (Get-Content $EnvironmentJsonPath -Raw | ConvertFrom-Json).connectionString
}

$persistence = Join-Path $repoRoot "RaphCare.Persistence\RaphCare.Persistence.csproj"
$startup = Join-Path $repoRoot "RaphCare.API\RaphCare.API.csproj"

$contexts = @(
    "ClinicalDbContext",
    "DeviceDbContext",
    "InsuranceDbContext",
    "BillingDbContext",
    "AIDbContext",
    "IdentityDbContext"
)

dotnet tool list -g | Out-Null
$ef = dotnet ef --version 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "Installing dotnet-ef ..."
    dotnet tool install -g dotnet-ef
}

foreach ($ctx in $contexts) {
    Write-Host "Migrating $ctx ..."
    dotnet ef database update `
        --project $persistence `
        --startup-project $startup `
        --context $ctx `
        --connection $ConnectionString
    if ($LASTEXITCODE -ne 0) {
        throw "Migration failed for $ctx"
    }
}

Write-Host "All contexts migrated."
Write-Host "Note: Development-only seed data is not applied in Production. Seed test users via Entra roles / admin flows as needed."
