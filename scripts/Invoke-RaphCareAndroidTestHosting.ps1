<#
.SYNOPSIS
  Orchestrates Azure test hosting after `az login` (provision → optional Entra → settings → migrate → publish).
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $SqlAdminPassword,
    [Parameter(Mandatory = $true)]
    [string] $JwtSecret,
    [string] $SqlAdminUser = "raphcaresqladmin",
    [string] $Location = "southafricanorth",
    [string] $SmtpPassword = "",
    [switch] $SkipEntra,
    [switch] $SkipPublish,
    [switch] $SkipMigrate
)

$ErrorActionPreference = "Stop"
$scripts = $PSScriptRoot

az account show -o none 2>$null
if ($LASTEXITCODE -ne 0) { throw "Run az login first, then re-run this script." }

& (Join-Path $scripts "New-RaphCareAzureTestEnvironment.ps1") `
    -Location $Location `
    -SqlAdminUser $SqlAdminUser `
    -SqlAdminPassword $SqlAdminPassword
if ($LASTEXITCODE -ne 0) { throw "Provisioning failed" }

$envPath = Join-Path (Split-Path -Parent $scripts) "artifacts\azure-test-environment.json"

if (-not $SkipEntra) {
    & (Join-Path $scripts "New-RaphCareEntraTestApps.ps1") -EnvironmentJsonPath $envPath
    if ($LASTEXITCODE -ne 0) { throw "Entra registration failed" }
}

$envInfo = Get-Content $envPath -Raw | ConvertFrom-Json
if (-not $envInfo.apiAppClientId -or -not $envInfo.entraTenantId) {
    throw "Missing Entra ids in environment JSON. Run New-RaphCareEntraTestApps.ps1 or pass values to Set-RaphCareAzureTestAppSettings.ps1 manually."
}

$setArgs = @{
    EnvironmentJsonPath = $envPath
    TenantId            = $envInfo.entraTenantId
    ApiAppClientId      = $envInfo.apiAppClientId
    JwtSecret           = $JwtSecret
    WebPortalBaseUrl    = $envInfo.webPortalBaseUrl
}
if ($SmtpPassword) { $setArgs.SmtpPassword = $SmtpPassword }

& (Join-Path $scripts "Set-RaphCareAzureTestAppSettings.ps1") @setArgs
if ($LASTEXITCODE -ne 0) { throw "App settings failed" }

if (-not $SkipMigrate) {
    & (Join-Path $scripts "Update-RaphCareAzureSqlMigrations.ps1") -EnvironmentJsonPath $envPath
    if ($LASTEXITCODE -ne 0) { throw "Migrations failed" }
}

if (-not $SkipPublish) {
    & (Join-Path $scripts "Publish-RaphCareApiToAzure.ps1") -EnvironmentJsonPath $envPath
    if ($LASTEXITCODE -ne 0) { throw "API publish failed" }
    & (Join-Path $scripts "Publish-RaphCareWebToAzure.ps1") -EnvironmentJsonPath $envPath
    if ($LASTEXITCODE -ne 0) { throw "Web publish failed" }
}

Write-Host ""
Write-Host "Azure test host pipeline finished."
Write-Host "API: $($envInfo.apiBaseUrl)"
Write-Host "Web: $($envInfo.webPortalBaseUrl)"
Write-Host "Next: New-RaphCareAndroidUploadKeystore.ps1 then Publish-RaphCareAndroidPlay.ps1, then Play Internal upload."
Write-Host "Docs: docs/Mobile_Android_Test_Hosting.md"
