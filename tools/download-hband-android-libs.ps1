#Requires -Version 5.1
<#
  Downloads HBand / Veepoo Android SDK artifacts from the public GitHub repo
  https://github.com/HBandSDK/Android_Ble_SDK (Apache 2.0).

  Run from the repository root:
    .\tools\download-hband-android-libs.ps1

  Filenames track the repo's jar_core layout (versions change).
  See docs/12_HBand_SDK_Integration.md.
#>
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path $PSScriptRoot -Parent
$dest = Join-Path $repoRoot 'RaphCare.Mobile\Platforms\Android\libs'

# Prefer raw GitHub (no REST API, avoids unauthenticated rate limits). jsDelivr is the fallback.
# gson is provided by the GoogleGson NuGet package in RaphCare.Mobile.csproj (do not embed the jar).
$files = @(
    'vpbluetooth-1.20.aar',
    'vpprotocol-2.3.81.15.aar',
    'JL_Watch_V1.13.1_11214-release.aar',
    'jl_rcsp_V0.7.2_527-release.aar',
    'jl_bt_ota_V1.10.0_10931-release.aar',
    'BmpConvert_V1.6.0_10604-release.aar',
    'abpartool-release.aar'
)

$bases = @(
    'https://raw.githubusercontent.com/HBandSDK/Android_Ble_SDK/master/android_sdk_source/jar_core',
    'https://cdn.jsdelivr.net/gh/HBandSDK/Android_Ble_SDK@master/android_sdk_source/jar_core'
)

function Download-HbandFile {
    param(
        [string] $Name,
        [string] $OutPath
    )

    $headers = @{ 'User-Agent' = 'RaphCare-HBand-download' }
    foreach ($base in $bases) {
        $uri = "$base/$Name"
        Write-Host "Downloading $Name from $base ..."
        try {
            Invoke-WebRequest -Uri $uri -OutFile $OutPath -UseBasicParsing -Headers $headers
            if ((Get-Item $OutPath).Length -ge 1024) {
                return
            }
        }
        catch {
            Write-Host "  failed: $($_.Exception.Message)"
        }
    }

    throw "Could not download $Name. Update the filename in tools/download-hband-android-libs.ps1 to match https://github.com/HBandSDK/Android_Ble_SDK/tree/master/android_sdk_source/jar_core"
}

New-Item -ItemType Directory -Force -Path $dest | Out-Null
Get-ChildItem -Path $dest -Filter 'vpprotocol-*.aar' -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -ne 'vpprotocol-2.3.81.15.aar' } |
    Remove-Item -Force
foreach ($name in $files) {
    Download-HbandFile -Name $name -OutPath (Join-Path $dest $name)
}
Write-Host "Done. Files in: $dest"
Write-Host "Next: also fetch Nordic mcumgr AARs (required for Veepoo connect OTA init):"
Write-Host "  .\tools\download-hband-nordic-mcumgr-libs.ps1"
Write-Host "Then rebuild Android; C# calls VPOperateManager via Platforms/Android/HBand (JNI). See docs/12."
