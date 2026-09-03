<#
.SYNOPSIS
  Sets App Service settings on the admin web host (raphcare) so Blazor WASM calls the hosted API.
.NOTES
  Azure app settings apply to RaphCare.Portal.Host, not to the browser by themselves.
  The host forwards ASPNETCORE_ENVIRONMENT (Blazor-Environment) and ApiBaseUrl.
  SMTP belongs on raphcare-api, not on this web app.
#>
[CmdletBinding()]
param(
    [string] $WebAppName = "raphcare",
    [string] $ResourceGroup = "raphcare_group",
    [string] $ApiBaseUrl = "",
    [string] $EnvironmentName = "Staging",
    [string] $EnvironmentJsonPath = ""
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
if (-not $EnvironmentJsonPath) {
    $EnvironmentJsonPath = Join-Path $repoRoot "artifacts\azure-test-environment.json"
}

az account show -o none 2>$null
if ($LASTEXITCODE -ne 0) { throw "Azure CLI is not logged in. Run: az login" }

if (-not $ApiBaseUrl -and (Test-Path $EnvironmentJsonPath)) {
    $envInfo = Get-Content $EnvironmentJsonPath -Raw | ConvertFrom-Json
    if ($envInfo.apiBaseUrl) { $ApiBaseUrl = [string]$envInfo.apiBaseUrl }
    if ($envInfo.resourceGroup) { $ResourceGroup = [string]$envInfo.resourceGroup }
    if ($envInfo.webAppName) { $WebAppName = [string]$envInfo.webAppName }
}

if (-not $ApiBaseUrl) {
    $ApiBaseUrl = "https://raphcare-api-eydjcnefhae2dpa2.southafricanorth-01.azurewebsites.net"
}

$ApiBaseUrl = $ApiBaseUrl.Trim().TrimEnd('/')

Write-Host "Updating App Service settings for $WebAppName ..."
az webapp config appsettings set `
    --resource-group $ResourceGroup `
    --name $WebAppName `
    --settings `
        "ASPNETCORE_ENVIRONMENT=$EnvironmentName" `
        "ApiBaseUrl=$ApiBaseUrl" `
    -o none
if ($LASTEXITCODE -ne 0) { throw "Failed to set App Service settings for $WebAppName" }

Write-Host "ASPNETCORE_ENVIRONMENT=$EnvironmentName"
Write-Host "ApiBaseUrl=$ApiBaseUrl"
