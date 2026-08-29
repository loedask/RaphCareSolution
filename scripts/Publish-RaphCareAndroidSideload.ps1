<#
.SYNOPSIS
  Builds a Release sideload APK (debug-signed) pointed at the staging API, with a versioned file name.

.EXAMPLE
  powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Publish-RaphCareAndroidSideload.ps1

.EXAMPLE
  # Bump Android versionCode in the csproj, then build
  powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Publish-RaphCareAndroidSideload.ps1 -BumpBuild
#>
[CmdletBinding()]
param(
    [string] $ApiBaseAddress = "",
    [string] $EnvironmentJsonPath = "",
    [switch] $BumpBuild
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
. (Join-Path $PSScriptRoot "RaphCareMobileVersion.ps1")

if ($BumpBuild) {
    $version = Step-RaphCareMobileVersionCode -RepoRoot $repoRoot
    Write-Host "Bumped ApplicationVersion to $($version.VersionCode) ($($version.Label))"
}
else {
    $version = Get-RaphCareMobileVersion -RepoRoot $repoRoot
}

if (-not $EnvironmentJsonPath) {
    $EnvironmentJsonPath = Join-Path $repoRoot "artifacts\azure-test-environment.json"
}
if (-not $ApiBaseAddress -and (Test-Path $EnvironmentJsonPath)) {
    $ApiBaseAddress = (Get-Content $EnvironmentJsonPath -Raw | ConvertFrom-Json).apiBaseUrlSlash
}
if (-not $ApiBaseAddress) {
    $testHosting = Get-Content (Join-Path $repoRoot "RaphCare.Mobile\appsettings.TestHosting.json") -Raw | ConvertFrom-Json
    $ApiBaseAddress = $testHosting.Api.BaseAddress
}
if (-not $ApiBaseAddress) {
    throw "Provide -ApiBaseAddress or ensure appsettings.TestHosting.json / azure-test-environment.json has the API URL."
}
if (-not $ApiBaseAddress.EndsWith('/')) { $ApiBaseAddress += '/' }

$testHostingPath = Join-Path $repoRoot "RaphCare.Mobile\appsettings.TestHosting.json"
$testHosting = Get-Content $testHostingPath -Raw | ConvertFrom-Json
$testHosting.Api.BaseAddress = $ApiBaseAddress
$testHosting | ConvertTo-Json -Depth 5 | Set-Content $testHostingPath -Encoding UTF8

$artifactsDir = Join-Path $repoRoot "artifacts\android"
$apkDir = Join-Path $artifactsDir "publish-apk"
Remove-Item -Recurse -Force $apkDir -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $apkDir | Out-Null

if (-not $env:JAVA_HOME -or -not (Test-Path $env:JAVA_HOME)) {
    $candidate = "C:\Program Files\Eclipse Adoptium\jdk-21.0.6.7-hotspot"
    if (Test-Path $candidate) { $env:JAVA_HOME = $candidate }
}
if (-not $env:ANDROID_HOME) {
    $sdk = Join-Path ${env:ProgramFiles(x86)} "Android\android-sdk"
    if (Test-Path $sdk) {
        $env:ANDROID_HOME = $sdk
        $env:ANDROID_SDK_ROOT = $sdk
    }
}

$csproj = Join-Path $repoRoot "RaphCare.Mobile\RaphCare.Mobile.csproj"
Write-Host "Publishing sideload APK $($version.Label) (Api:BaseAddress=$ApiBaseAddress) ..."

dotnet publish $csproj `
    -f net10.0-android `
    -c Release `
    -o $apkDir `
    --property:AndroidPackageFormat=apk `
    --property:WarningsNotAsErrors=XC0022%3BXC0025

if ($LASTEXITCODE -ne 0) { throw "Android sideload APK publish failed" }

$binApkRoot = Join-Path $repoRoot "RaphCare.Mobile\bin\Release\net10.0-android"
Copy-RaphCareAndroidArtifacts -ArtifactsDir $artifactsDir -FileStem $version.FileStem -SearchRoots @($apkDir, $binApkRoot) | Out-Null

Write-Host ""
Write-Host "Sideload this file (uninstall any older RaphCare build first if install fails):"
Write-Host "  $(Join-Path $artifactsDir ($version.FileStem + '.apk'))"
Write-Host "Package id: com.yindula.raphcare"
Write-Host "Display version $($version.DisplayVersion), build $($version.VersionCode)"
