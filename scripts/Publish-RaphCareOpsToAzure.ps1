<#
.SYNOPSIS
  Publishes RaphCare.Ops.Host (Linux) and zip-deploys it to App Service "raphcare-ops".
.NOTES
  Same host pattern as Portal: do not publish the WASM project alone to Linux App Service.
#>
[CmdletBinding()]
param(
    [string] $EnvironmentJsonPath = "",
    [string] $WebAppName = "raphcare-ops",
    [string] $ResourceGroup = "raphcare_group",
    [string] $ApiBaseUrl = ""
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
if (-not $EnvironmentJsonPath) {
    $EnvironmentJsonPath = Join-Path $repoRoot "artifacts\azure-test-environment.json"
}

az account show -o none 2>$null
if ($LASTEXITCODE -ne 0) {
    throw "Azure CLI is not logged in. Run: az login (interactive browser). See docs/Mobile_Android_Test_Hosting.md Phase 1. Do not use --tenant with --scope and --use-device-code."
}

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
    if ($envInfo.opsAppName) { $WebAppName = [string]$envInfo.opsAppName }
}

Write-Host "ApiBaseUrl -> $ApiBaseUrl"

$publishDir = Join-Path $repoRoot "artifacts\publish-ops-host"
$zipPath = Join-Path $repoRoot "artifacts\raphcare-ops-host.zip"
Remove-Item -Recurse -Force $publishDir -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $publishDir | Out-Null

Write-Host "dotnet publish RaphCare.Ops.Host (linux-x64) ..."
dotnet publish (Join-Path $repoRoot "RaphCare.Ops.Host\RaphCare.Ops.Host.csproj") `
    -c Release `
    -r linux-x64 `
    --self-contained false `
    -o $publishDir
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed" }

$apiBaseJson = @{ ApiBaseUrl = $ApiBaseUrl } | ConvertTo-Json
foreach ($name in @("appsettings.json", "appsettings.Staging.json")) {
    $publishedSettings = Join-Path $publishDir "wwwroot\$name"
    if (-not (Test-Path (Split-Path $publishedSettings))) {
        throw "Published wwwroot not found under $publishDir"
    }
    Set-Content -Path $publishedSettings -Value $apiBaseJson -Encoding UTF8
}

. (Join-Path $PSScriptRoot "Compress-RaphCareUnixZip.ps1")
Compress-RaphCareUnixZip -SourceDir $publishDir -ZipPath $zipPath

Write-Host "Deploying to $WebAppName ..."
az webapp deploy `
    --resource-group $ResourceGroup `
    --name $WebAppName `
    --src-path $zipPath `
    --type zip `
    --clean true `
    --restart true `
    -o none
if ($LASTEXITCODE -ne 0) { throw "az webapp deploy failed for $WebAppName" }

& (Join-Path $PSScriptRoot "Set-RaphCareAzureWebAppSettings.ps1") `
    -WebAppName $WebAppName `
    -ResourceGroup $ResourceGroup `
    -ApiBaseUrl $ApiBaseUrl `
    -EnvironmentName "Staging"

Write-Host "Ops host published to App Service $WebAppName"
