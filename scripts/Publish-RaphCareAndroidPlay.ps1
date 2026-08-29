<#
.SYNOPSIS
  Builds a signed Release Android App Bundle (and APK) for Play Internal testing.
  Output names use ApplicationDisplayVersion + ApplicationVersion, e.g. RaphCare-v1.0.0+2.aab.
#>
[CmdletBinding()]
param(
    [string] $EnvironmentJsonPath = "",
    [string] $KeystorePath = "",
    [string] $Alias = "raphcare",
    [Parameter(Mandatory = $true)]
    [string] $KeystorePassword,
    [Parameter(Mandatory = $true)]
    [string] $KeyPassword,
    [string] $ApiBaseAddress = "",
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

$artifactsDir = Join-Path $repoRoot "artifacts\android"
New-Item -ItemType Directory -Force -Path $artifactsDir | Out-Null

if (-not $EnvironmentJsonPath) {
    $EnvironmentJsonPath = Join-Path $repoRoot "artifacts\azure-test-environment.json"
}
if (-not $KeystorePath) {
    $KeystorePath = Join-Path $repoRoot "artifacts\android-signing\raphcare-upload.keystore"
}
if (-not (Test-Path $KeystorePath)) {
    throw "Keystore not found at $KeystorePath. Run New-RaphCareAndroidUploadKeystore.ps1 first."
}

if (-not $ApiBaseAddress -and (Test-Path $EnvironmentJsonPath)) {
    $ApiBaseAddress = (Get-Content $EnvironmentJsonPath -Raw | ConvertFrom-Json).apiBaseUrlSlash
}
if (-not $ApiBaseAddress) {
    throw "Provide -ApiBaseAddress or run New-RaphCareAzureTestEnvironment.ps1 first."
}
if (-not $ApiBaseAddress.EndsWith('/')) { $ApiBaseAddress += '/' }

$testHostingPath = Join-Path $repoRoot "RaphCare.Mobile\appsettings.TestHosting.json"
$testHosting = Get-Content $testHostingPath -Raw | ConvertFrom-Json
$testHosting.Api.BaseAddress = $ApiBaseAddress
$testHosting | ConvertTo-Json -Depth 5 | Set-Content $testHostingPath -Encoding UTF8

$csproj = Join-Path $repoRoot "RaphCare.Mobile\RaphCare.Mobile.csproj"
$publishDir = Join-Path $artifactsDir "publish"
Remove-Item -Recurse -Force $publishDir -ErrorAction SilentlyContinue

Write-Host "Publishing net10.0-android Release AAB $($version.Label) (Api:BaseAddress=$ApiBaseAddress) ..."
dotnet publish $csproj `
    -f net10.0-android `
    -c Release `
    -o $publishDir `
    /p:AndroidPackageFormat=aab `
    /p:WarningsNotAsErrors=XC0022`;XC0025 `
    /p:RaphCareAndroidKeystore="$KeystorePath" `
    /p:RaphCareAndroidKeystorePass="$KeystorePassword" `
    /p:RaphCareAndroidKeyAlias="$Alias" `
    /p:RaphCareAndroidKeyPass="$KeyPassword"

if ($LASTEXITCODE -ne 0) { throw "Android AAB publish failed" }

Write-Host "Also publishing APK for optional sideload ..."
$apkDir = Join-Path $artifactsDir "publish-apk"
Remove-Item -Recurse -Force $apkDir -ErrorAction SilentlyContinue
dotnet publish $csproj `
    -f net10.0-android `
    -c Release `
    -o $apkDir `
    /p:AndroidPackageFormat=apk `
    /p:WarningsNotAsErrors=XC0022`;XC0025 `
    /p:RaphCareAndroidKeystore="$KeystorePath" `
    /p:RaphCareAndroidKeystorePass="$KeystorePassword" `
    /p:RaphCareAndroidKeyAlias="$Alias" `
    /p:RaphCareAndroidKeyPass="$KeyPassword"

if ($LASTEXITCODE -ne 0) { throw "Android APK publish failed" }

Copy-RaphCareAndroidArtifacts -ArtifactsDir $artifactsDir -FileStem $version.FileStem -SearchRoots @($publishDir, $apkDir) | Out-Null

Write-Host ""
Write-Host "Upload $(Join-Path $artifactsDir ($version.FileStem + '.aab')) in Play Console > Testing > Internal testing."
Write-Host "Package id: com.yindula.raphcare"
Write-Host "Display version $($version.DisplayVersion), build $($version.VersionCode)"
Write-Host "See docs/Mobile_Android_Test_Hosting.md"
