<#
.SYNOPSIS
  Publishes RaphCare.API and zip-deploys it to the test App Service.
#>
[CmdletBinding()]
param(
    [string] $EnvironmentJsonPath = ""
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
if (-not $EnvironmentJsonPath) {
    $EnvironmentJsonPath = Join-Path $repoRoot "artifacts\azure-test-environment.json"
}
if (-not (Test-Path $EnvironmentJsonPath)) {
    throw "Missing $EnvironmentJsonPath. Run New-RaphCareAzureTestEnvironment.ps1 first."
}

az account show -o none 2>$null
if ($LASTEXITCODE -ne 0) { throw "Azure CLI is not logged in. Run: az login" }

$envInfo = Get-Content $EnvironmentJsonPath -Raw | ConvertFrom-Json
$publishDir = Join-Path $repoRoot "artifacts\publish-api"
$zipPath = Join-Path $repoRoot "artifacts\raphcare-api.zip"
Remove-Item -Recurse -Force $publishDir -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $publishDir | Out-Null

Write-Host "dotnet publish RaphCare.API ..."
dotnet publish (Join-Path $repoRoot "RaphCare.API\RaphCare.API.csproj") `
    -c Release `
    -o $publishDir
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed" }

. (Join-Path $PSScriptRoot "Compress-RaphCareUnixZip.ps1")
Compress-RaphCareUnixZip -SourceDir $publishDir -ZipPath $zipPath

Write-Host "Deploying to $($envInfo.apiAppName) ..."
az webapp deploy `
    --resource-group $envInfo.resourceGroup `
    --name $envInfo.apiAppName `
    --src-path $zipPath `
    --type zip `
    --clean true `
    --restart true `
    -o none
if ($LASTEXITCODE -ne 0) { throw "az webapp deploy failed for $($envInfo.apiAppName)" }

Write-Host "Deployed. API base: $($envInfo.apiBaseUrl)"
