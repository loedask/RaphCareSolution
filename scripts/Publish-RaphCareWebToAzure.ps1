<#
.SYNOPSIS
  Publishes RaphCare.Web.Host (Linux) and zip-deploys it to App Service "raphcare".
.NOTES
  Do not publish RaphCare.Web alone to Linux App Service. Standalone Blazor WASM is static files;
  the thin host provides SPA fallback and correct framework MIME types (same pattern as Bobeta.Web.Host).
#>
[CmdletBinding()]
param(
    [string] $EnvironmentJsonPath = "",
    [string] $WebAppName = "raphcare",
    [string] $ResourceGroup = "raphcare_group",
    [string] $ApiBaseUrl = ""
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
if (-not $EnvironmentJsonPath) {
    $EnvironmentJsonPath = Join-Path $repoRoot "artifacts\azure-test-environment.json"
}

az account show -o none 2>$null
if ($LASTEXITCODE -ne 0) { throw "Azure CLI is not logged in. Run: az login" }

if (-not $ApiBaseUrl -and (Test-Path $EnvironmentJsonPath)) {
    $ApiBaseUrl = (Get-Content $EnvironmentJsonPath -Raw | ConvertFrom-Json).apiBaseUrl
}
if (-not $ApiBaseUrl) {
    $ApiBaseUrl = "https://raphcare-api-eydjcnefhae2dpa2.southafricanorth-01.azurewebsites.net"
}
$ApiBaseUrl = $ApiBaseUrl.TrimEnd('/')

if (Test-Path $EnvironmentJsonPath) {
    $envInfo = Get-Content $EnvironmentJsonPath -Raw | ConvertFrom-Json
    if ($envInfo.resourceGroup) { $ResourceGroup = $envInfo.resourceGroup }
}

$webAppSettingsPath = Join-Path $repoRoot "RaphCare.Web\wwwroot\appsettings.json"
@{ ApiBaseUrl = $ApiBaseUrl } | ConvertTo-Json | Set-Content $webAppSettingsPath -Encoding UTF8
Write-Host "ApiBaseUrl -> $ApiBaseUrl"

$publishDir = Join-Path $repoRoot "artifacts\publish-web-host"
$zipPath = Join-Path $repoRoot "artifacts\raphcare-web-host.zip"
Remove-Item -Recurse -Force $publishDir -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $publishDir | Out-Null

Write-Host "dotnet publish RaphCare.Web.Host (linux-x64) ..."
dotnet publish (Join-Path $repoRoot "RaphCare.Web.Host\RaphCare.Web.Host.csproj") `
    -c Release `
    -r linux-x64 `
    --self-contained false `
    -o $publishDir
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed" }

if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
Compress-Archive -Path (Join-Path $publishDir "*") -DestinationPath $zipPath -Force

Write-Host "Deploying to $WebAppName ..."
az webapp deploy `
    --resource-group $ResourceGroup `
    --name $WebAppName `
    --src-path $zipPath `
    --type zip `
    -o none

Write-Host "Web host published to App Service $WebAppName"
