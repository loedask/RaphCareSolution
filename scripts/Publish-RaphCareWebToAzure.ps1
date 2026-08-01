<#
.SYNOPSIS
  Publishes RaphCare.Web (Blazor WASM) to the Azure Storage static website ($web).
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
$publishDir = Join-Path $repoRoot "artifacts\publish-web"
Remove-Item -Recurse -Force $publishDir -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $publishDir | Out-Null

Write-Host "dotnet publish RaphCare.Web ..."
dotnet publish (Join-Path $repoRoot "RaphCare.Web\RaphCare.Web.csproj") `
    -c Release `
    -o $publishDir
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed" }

$wwwroot = Join-Path $publishDir "wwwroot"
if (-not (Test-Path $wwwroot)) {
    throw "Expected wwwroot under $publishDir"
}

$appsettingsPath = Join-Path $wwwroot "appsettings.json"
@{ ApiBaseUrl = $envInfo.apiBaseUrl.TrimEnd('/') } | ConvertTo-Json | Set-Content $appsettingsPath -Encoding UTF8

Write-Host "Uploading to `$web on $($envInfo.storageAccountName) ..."
az storage blob upload-batch `
    --account-name $envInfo.storageAccountName `
    --destination '$web' `
    --source $wwwroot `
    --overwrite true `
    -o none

Write-Host "Web published: $($envInfo.webPortalBaseUrl)"
