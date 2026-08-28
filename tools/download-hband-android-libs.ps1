#Requires -Version 5.1
<#
  Downloads HBand / Veepoo Android SDK artifacts from the public GitHub repo
  https://github.com/HBandSDK/Android_Ble_SDK (Apache 2.0).

  Run from the repository root:
    .\tools\download-hband-android-libs.ps1

  Filenames track the repo's current jar_base / jar_core layout (versions change).
  See docs/12_HBand_SDK_Integration.md.
#>
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path $PSScriptRoot -Parent
$base = 'https://raw.githubusercontent.com/HBandSDK/Android_Ble_SDK/master/android_sdk_source'
$dest = Join-Path $repoRoot 'RaphCare.Mobile\Platforms\Android\libs'

# Minimal set for connect + password + person sync + HR / SpO2 (not OTA / Goodix DFU).
# gson is provided by the GoogleGson NuGet package in RaphCare.Mobile.csproj (do not embed the jar).
$files = @(
    @{ Path = 'jar_core/vpbluetooth-1.20.aar'; Out = 'vpbluetooth-1.20.aar' },
    @{ Path = 'jar_core/vpprotocol-2.3.71.15.aar'; Out = 'vpprotocol-2.3.71.15.aar' },
    @{ Path = 'jar_core/JL_Watch_V1.13.1_11214-release.aar'; Out = 'JL_Watch_V1.13.1_11214-release.aar' },
    @{ Path = 'jar_core/jl_rcsp_V0.7.2_527-release.aar'; Out = 'jl_rcsp_V0.7.2_527-release.aar' },
    @{ Path = 'jar_core/jl_bt_ota_V1.10.0_10931-release.aar'; Out = 'jl_bt_ota_V1.10.0_10931-release.aar' },
    @{ Path = 'jar_core/BmpConvert_V1.6.0_10604-release.aar'; Out = 'BmpConvert_V1.6.0_10604-release.aar' },
    @{ Path = 'jar_core/abpartool-release.aar'; Out = 'abpartool-release.aar' }
)

New-Item -ItemType Directory -Force -Path $dest | Out-Null
foreach ($f in $files) {
    $uri = "$base/$($f.Path)"
    $out = Join-Path $dest $f.Out
    Write-Host "Downloading $($f.Out) ..."
    Invoke-WebRequest -Uri $uri -OutFile $out -UseBasicParsing
}
Write-Host "Done. Files in: $dest"
Write-Host "Next: rebuild Android; C# calls VPOperateManager via Platforms/Android/HBand (JNI). See docs/12."
