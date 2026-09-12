<#
.SYNOPSIS
  Capture Android logcat while reproducing a RaphCare Measure / watch BLE crash.

.DESCRIPTION
  Clears the device log, waits for you to reproduce the issue, then writes a filtered
  log under artifacts/android/logs/. Use this when Measure force-closes the app so we
  can see Veepoo / Inuker / AndroidRuntime lines.

.EXAMPLE
  powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Capture-RaphCareAndroidLogcat.ps1
#>
[CmdletBinding()]
param(
    [int]$WaitSeconds = 90,
    [string]$AdbPath = ""
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot

function Find-Adb {
    param([string]$Explicit)
    if ($Explicit -and (Test-Path -LiteralPath $Explicit)) {
        return (Resolve-Path -LiteralPath $Explicit).Path
    }
    $cmd = Get-Command adb -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }

    $candidates = [System.Collections.Generic.List[string]]::new()
    if ($env:LOCALAPPDATA) {
        $candidates.Add((Join-Path $env:LOCALAPPDATA "Android\Sdk\platform-tools\adb.exe"))
    }
    if ($env:ANDROID_HOME) {
        $candidates.Add((Join-Path $env:ANDROID_HOME "platform-tools\adb.exe"))
    }
    if ($env:ANDROID_SDK_ROOT) {
        $candidates.Add((Join-Path $env:ANDROID_SDK_ROOT "platform-tools\adb.exe"))
    }
    # Common Windows install from older Android Studio / SDK Manager layouts.
    $candidates.Add("${env:ProgramFiles(x86)}\Android\android-sdk\platform-tools\adb.exe")
    $candidates.Add("$env:ProgramFiles\Android\android-sdk\platform-tools\adb.exe")
    foreach ($c in $candidates) {
        if ($c -and (Test-Path -LiteralPath $c)) { return $c }
    }
    return $null
}

$adb = Find-Adb -Explicit $AdbPath
if (-not $adb) {
    Write-Error "adb not found. Install Android platform-tools or pass -AdbPath to adb.exe."
}

Write-Host "Using adb: $adb"
& $adb start-server | Out-Null
$devices = & $adb devices
Write-Host ($devices -join "`n")
$ready = @($devices | Where-Object { $_ -match "`tdevice$" })
if ($ready.Count -lt 1) {
    Write-Error "No Android device in 'device' state. Enable USB debugging and accept the prompt."
}

$outDir = Join-Path $repoRoot "artifacts\android\logs"
New-Item -ItemType Directory -Force -Path $outDir | Out-Null
$stamp = Get-Date -Format "yyyyMMdd-HHmmss"
$outFile = Join-Path $outDir "raphcare-logcat-$stamp.txt"

Write-Host "Clearing logcat..."
& $adb logcat -c | Out-Null

Write-Host ""
Write-Host "Reproduce now (start this script BEFORE tapping on the phone):"
Write-Host "  Probe build (1.8.46+): Devices → Try vendor scan (diagnostic). Do NOT use normal Scan first."
Write-Host "  Stable Connect (1.8.42 style): Devices → Scan → Connect (Plugin.BLE only)."
Write-Host "  If a vendor session survives: Watch readings → Measure."
Write-Host "Capturing for $WaitSeconds seconds..."
Write-Host "Output: $outFile"
Write-Host "Inspect later for:"
Write-Host "  - RaphCareHBand SCAN-* / CONNECT-2 without CONNECT-3"
Write-Host "  - AndroidRuntime / Fatal signal"
Write-Host "  - DEBUG tombstone lines naming the crashing .so or symbol"
Write-Host ""

# adb logcat filterspecs must be separate argv tokens (not one quoted blob).
# Tag names are exact-match; wildcards like RaphCare*:V are invalid.
# DEBUG:E is where debuggerd writes native SIGSEGV/SIGABRT tombstones.
# Pass one ArgumentList string on Windows so CreateProcess tokenizes correctly
# (an array with a space-joined filterspec was passed as a single malformed arg).
$argLine = "logcat -v threadtime RaphCareHBand:V AndroidRuntime:E DEBUG:E mono-rt:E libc:F chromium:S"
$proc = Start-Process -FilePath $adb -ArgumentList $argLine `
    -RedirectStandardOutput $outFile -NoNewWindow -PassThru

try {
    Start-Sleep -Seconds $WaitSeconds
}
finally {
    if (-not $proc.HasExited) {
        Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
    }
}

Write-Host "Done. Share this file with the engineer:"
Write-Host "  $outFile"
if ((Get-Item -LiteralPath $outFile).Length -lt 64) {
    Write-Warning "Log file looks empty. Try again with the phone unlocked and USB debugging authorized."
    Write-Warning "Also confirm USB debugging is authorized and the device stayed connected for the whole capture."
}
