<#
.SYNOPSIS
  Sets RaphCare__OpsBaseUrl and Cors__OpsOrigins__0 on the API App Service, then restarts the API.
#>
[CmdletBinding()]
param(
    [string] $EnvironmentJsonPath = "",
    [string] $ResourceGroup = "raphcare_group",
    [string] $ApiAppName = "raphcare-api",
    [string] $OpsBaseUrl = "",
    [string] $OpsAppName = "raphcare-ops"
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
if (-not $EnvironmentJsonPath) {
    $EnvironmentJsonPath = Join-Path $repoRoot "artifacts\azure-test-environment.json"
}

az account show -o none 2>$null
if ($LASTEXITCODE -ne 0) {
    throw "Azure CLI is not logged in. Run: az login (interactive browser)."
}

if (Test-Path $EnvironmentJsonPath) {
    $envInfo = Get-Content $EnvironmentJsonPath -Raw | ConvertFrom-Json
    if ($envInfo.resourceGroup) { $ResourceGroup = [string]$envInfo.resourceGroup }
    if ($envInfo.apiAppName) { $ApiAppName = [string]$envInfo.apiAppName }
    if ($envInfo.opsAppName) { $OpsAppName = [string]$envInfo.opsAppName }
    if (-not $OpsBaseUrl -and $envInfo.opsBaseUrl) { $OpsBaseUrl = [string]$envInfo.opsBaseUrl }
}

if (-not $OpsBaseUrl) {
    $opsJson = cmd /c "az webapp show --resource-group `"$ResourceGroup`" --name `"$OpsAppName`" -o json 2>nul"
    if ($LASTEXITCODE -ne 0 -or -not $opsJson) {
        throw "Ops app '$OpsAppName' not found. Pass -OpsBaseUrl or run New-RaphCareOpsAzureApp.ps1 first."
    }
    $ops = $opsJson | ConvertFrom-Json
    $OpsBaseUrl = "https://$($ops.defaultHostName)"
}

$OpsBaseUrl = $OpsBaseUrl.Trim().TrimEnd('/')

Write-Host "Setting Ops CORS on $ApiAppName -> $OpsBaseUrl"
az webapp config appsettings set `
    --resource-group $ResourceGroup `
    --name $ApiAppName `
    --settings `
        "RaphCare__OpsBaseUrl=$OpsBaseUrl" `
        "Cors__OpsOrigins__0=$OpsBaseUrl" `
    -o none
if ($LASTEXITCODE -ne 0) { throw "Failed to set Ops CORS on $ApiAppName" }

az webapp restart --resource-group $ResourceGroup --name $ApiAppName -o none
if ($LASTEXITCODE -ne 0) { throw "Failed to restart $ApiAppName" }

if (Test-Path $EnvironmentJsonPath) {
    $envInfo = Get-Content $EnvironmentJsonPath -Raw | ConvertFrom-Json
    $envInfo | Add-Member -NotePropertyName opsBaseUrl -NotePropertyValue $OpsBaseUrl -Force
    $envInfo | Add-Member -NotePropertyName opsAppName -NotePropertyValue $OpsAppName -Force
    $envInfo | ConvertTo-Json -Depth 5 | Set-Content $EnvironmentJsonPath -Encoding UTF8
}

Write-Host "Ops CORS set. API restarted."
