<#
.SYNOPSIS
  Creates Linux App Service "raphcare-ops" on the same plan as the Portal web app (raphcare).
.NOTES
  Idempotent: if the app already exists, prints its URL and exits 0.
  After create, set API CORS (RaphCare__OpsBaseUrl / Cors__OpsOrigins__0) then publish with Publish-RaphCareOpsToAzure.ps1.
#>
[CmdletBinding()]
param(
    [string] $ResourceGroup = "raphcare_group",
    [string] $OpsAppName = "raphcare-ops",
    [string] $PortalWebAppName = "raphcare",
    [string] $EnvironmentJsonPath = ""
)

$ErrorActionPreference = "Stop"
# Windows PowerShell treats az stderr as a terminating error. Probe existence via cmd.
$repoRoot = Split-Path -Parent $PSScriptRoot
if (-not $EnvironmentJsonPath) {
    $EnvironmentJsonPath = Join-Path $repoRoot "artifacts\azure-test-environment.json"
}

az account show -o none 2>$null
if ($LASTEXITCODE -ne 0) {
    throw "Azure CLI is not logged in. Run: az login (interactive browser). See docs/Mobile_Android_Test_Hosting.md Phase 1."
}

if (Test-Path $EnvironmentJsonPath) {
    $envInfo = Get-Content $EnvironmentJsonPath -Raw | ConvertFrom-Json
    if ($envInfo.resourceGroup) { $ResourceGroup = [string]$envInfo.resourceGroup }
    if ($envInfo.webAppName) { $PortalWebAppName = [string]$envInfo.webAppName }
    if ($envInfo.opsAppName) { $OpsAppName = [string]$envInfo.opsAppName }
}

$existingJson = cmd /c "az webapp show --resource-group `"$ResourceGroup`" --name `"$OpsAppName`" -o json 2>nul"
if ($LASTEXITCODE -eq 0 -and $existingJson) {
    $app = $existingJson | ConvertFrom-Json
    Write-Host "Ops App Service already exists: $($app.defaultHostName)"
    exit 0
}

Write-Host "Looking up App Service plan for Portal app $PortalWebAppName ..."
$portalJson = az webapp show --resource-group $ResourceGroup --name $PortalWebAppName -o json 2>$null
if ($LASTEXITCODE -ne 0 -or -not $portalJson) {
    Write-Host "Portal app '$PortalWebAppName' not found by name. Searching resource group for a non-API Linux web app ..."
    $listJson = az webapp list --resource-group $ResourceGroup -o json
    if ($LASTEXITCODE -ne 0) { throw "Could not list web apps in $ResourceGroup." }
    $candidates = ($listJson | ConvertFrom-Json) | Where-Object {
        $_.name -ne $OpsAppName -and $_.name -notlike '*api*' -and $_.kind -like '*linux*'
    }
    if (-not $candidates -or @($candidates).Count -eq 0) {
        throw "Portal web app '$PortalWebAppName' not found in resource group '$ResourceGroup'."
    }
    $portal = @($candidates)[0]
    $PortalWebAppName = [string]$portal.name
    Write-Host "Using Portal app $PortalWebAppName"
}
else {
    $portal = $portalJson | ConvertFrom-Json
}

$planId = [string]$portal.serverFarmId
if (-not $planId) {
    throw "Could not read serverFarmId from $PortalWebAppName."
}

# az webapp create --plan accepts name; strip from resource id when needed.
$planName = ($planId -split '/')[-1]
Write-Host "Creating Ops web app $OpsAppName on plan $planName ..."
az webapp create `
    --resource-group $ResourceGroup `
    --plan $planName `
    --name $OpsAppName `
    --runtime "DOTNETCORE:10.0" `
    -o none
if ($LASTEXITCODE -ne 0) { throw "az webapp create failed for $OpsAppName" }

az webapp config set `
    --resource-group $ResourceGroup `
    --name $OpsAppName `
    --always-on true `
    -o none
if ($LASTEXITCODE -ne 0) { throw "az webapp config set failed for $OpsAppName" }

$created = az webapp show --resource-group $ResourceGroup --name $OpsAppName -o json | ConvertFrom-Json
$opsUrl = "https://$($created.defaultHostName)"
Write-Host "Created Ops App Service: $opsUrl"

if (Test-Path $EnvironmentJsonPath) {
    $envInfo = Get-Content $EnvironmentJsonPath -Raw | ConvertFrom-Json
    $envInfo | Add-Member -NotePropertyName opsAppName -NotePropertyValue $OpsAppName -Force
    $envInfo | Add-Member -NotePropertyName opsBaseUrl -NotePropertyValue $opsUrl -Force
    $envInfo | ConvertTo-Json -Depth 5 | Set-Content $EnvironmentJsonPath -Encoding UTF8
}

Write-Host "Next: set API CORS with Set-RaphCareAzureOpsCors.ps1, then Publish-RaphCareOpsToAzure.ps1"
