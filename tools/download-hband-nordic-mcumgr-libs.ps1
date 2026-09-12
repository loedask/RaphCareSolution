#Requires -Version 5.1
<#
  Downloads Nordic mcumgr AARs + transitive jars required by Veepoo connect
  (McuMgrOtaManager.init → McuMgrBleTransport).

  Run from the repository root:
    .\tools\download-hband-nordic-mcumgr-libs.ps1
#>
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path $PSScriptRoot -Parent
$dest = Join-Path $repoRoot 'RaphCare.Mobile\Platforms\Android\libs'
New-Item -ItemType Directory -Force -Path $dest | Out-Null

$artifacts = @(
    @{
        Name = 'mcumgr-ble-2.7.4.aar'
        Url  = 'https://repo1.maven.org/maven2/no/nordicsemi/android/mcumgr-ble/2.7.4/mcumgr-ble-2.7.4.aar'
    },
    @{
        Name = 'mcumgr-core-2.7.4.aar'
        Url  = 'https://repo1.maven.org/maven2/no/nordicsemi/android/mcumgr-core/2.7.4/mcumgr-core-2.7.4.aar'
    },
    @{
        Name = 'ble-2.11.0.aar'
        Url  = 'https://repo1.maven.org/maven2/no/nordicsemi/android/ble/2.11.0/ble-2.11.0.aar'
    },
    @{
        Name = 'slf4j-api-2.0.17.jar'
        Url  = 'https://repo1.maven.org/maven2/org/slf4j/slf4j-api/2.0.17/slf4j-api-2.0.17.jar'
    },
    @{
        Name = 'slf4j-nop-2.0.17.jar'
        Url  = 'https://repo1.maven.org/maven2/org/slf4j/slf4j-nop/2.0.17/slf4j-nop-2.0.17.jar'
    }
)

# Note: do not download kotlin-stdlib into libs. MAUI already includes Xamarin.Kotlin.StdLib;
# embedding another copy fails D8 with duplicate types.

$headers = @{ 'User-Agent' = 'RaphCare-HBand-download' }
foreach ($a in $artifacts) {
    $out = Join-Path $dest $a.Name
    Write-Host "Downloading $($a.Name) ..."
    Invoke-WebRequest -Uri $a.Url -OutFile $out -UseBasicParsing -Headers $headers
    if ((Get-Item $out).Length -lt 1024) {
        throw "Download too small: $($a.Name)"
    }
    Write-Host "  -> $((Get-Item $out).Length) bytes"
}

Write-Host "Done. Files in: $dest"
Write-Host "Rebuild Android. See docs/12_HBand_SDK_Integration.md."
