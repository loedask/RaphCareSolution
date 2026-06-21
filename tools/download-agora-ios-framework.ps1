# Downloads Agora iOS Video SDK (4.x) xcframework for RaphCare.Mobile native RTC.
# Run on macOS or Windows (extract only). Build/sign iOS on a Mac.
#
# Usage:
#   pwsh tools/download-agora-ios-framework.ps1
#
# Then open the solution on a Mac and build net10.0-ios. AgoraIosTelehealthRtcSession
# (when added) links against Platforms/iOS/AgoraRtcKit/AgoraRtcKit.xcframework.

$ErrorActionPreference = "Stop"
$version = "4.5.2"
$zipName = "AgoraRtcEngine_iOS-$version.zip"
$url = "https://download.agora.io/sdk/release/$zipName"
$root = Split-Path -Parent $PSScriptRoot
$destDir = Join-Path $root "RaphCare.Mobile\Platforms\iOS\AgoraRtcKit"
$zipPath = Join-Path $env:TEMP $zipName

Write-Host "Downloading Agora iOS SDK $version..."
Invoke-WebRequest -Uri $url -OutFile $zipPath -UseBasicParsing

$extractRoot = Join-Path $env:TEMP "AgoraRtcEngine_iOS-$version"
if (Test-Path $extractRoot) { Remove-Item $extractRoot -Recurse -Force }
Expand-Archive -Path $zipPath -DestinationPath $extractRoot -Force

$xc = Get-ChildItem -Path $extractRoot -Recurse -Filter "AgoraRtcKit.xcframework" | Select-Object -First 1
if (-not $xc) {
    Write-Error "AgoraRtcKit.xcframework not found in archive. Check Agora download layout for version $version."
}

if (Test-Path $destDir) { Remove-Item $destDir -Recurse -Force }
New-Item -ItemType Directory -Path $destDir -Force | Out-Null
Copy-Item -Path $xc.FullName -Destination (Join-Path $destDir "AgoraRtcKit.xcframework") -Recurse -Force

Write-Host "Installed: $destDir\AgoraRtcKit.xcframework"
Write-Host "Next: build RaphCare.Mobile for net10.0-ios on a Mac (NativeReference is conditional in csproj)."
