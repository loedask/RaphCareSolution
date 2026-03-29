#Requires -Version 5.1
<#
  Downloads minimal HBand Android SDK artifacts from the public GitHub repo
  https://github.com/HBandSDK/Android_Ble_SDK (Apache 2.0).
  Run from the repository root: .\tools\download-hband-android-libs.ps1
#>
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path $PSScriptRoot -Parent
$base = 'https://raw.githubusercontent.com/HBandSDK/Android_Ble_SDK/master/android_sdk_source'
$dest = Join-Path $repoRoot 'RaphCare.Mobile\Platforms\Android\libs'
$files = @(
    @{ Path = 'jar_base/gson-2.2.4.jar'; Out = 'gson-2.2.4.jar' },
    @{ Path = 'jar_base/vpbluetooth-1.18.aar'; Out = 'vpbluetooth-1.18.aar' },
    @{ Path = 'jar_core/vpprotocol-2.3.48.15.aar'; Out = 'vpprotocol-2.3.48.15.aar' }
)

New-Item -ItemType Directory -Force -Path $dest | Out-Null
foreach ($f in $files) {
    $uri = "$base/$($f.Path)"
    $out = Join-Path $dest $f.Out
    Write-Host "Downloading $($f.Out) ..."
    Invoke-WebRequest -Uri $uri -OutFile $out -UseBasicParsing
}
Write-Host "Done. Files in: $dest"
Write-Host "Next: read docs/12_HBand_SDK_Integration.md (binding project required for C#)."
