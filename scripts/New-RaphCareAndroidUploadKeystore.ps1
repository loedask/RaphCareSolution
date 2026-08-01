<#
.SYNOPSIS
  Creates an Android upload keystore for Play Internal testing (path is gitignored).
#>
[CmdletBinding()]
param(
    [string] $KeystorePath = "",
    [string] $Alias = "raphcare",
    [Parameter(Mandatory = $true)]
    [string] $KeystorePassword,
    [Parameter(Mandatory = $true)]
    [string] $KeyPassword,
    [string] $DistinguishedName = "CN=RaphCare, OU=Mobile, O=Yindula, L=Unknown, S=Unknown, C=ZA",
    [int] $ValidityDays = 10000
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$secretsDir = Join-Path $repoRoot "artifacts\android-signing"
New-Item -ItemType Directory -Force -Path $secretsDir | Out-Null

if (-not $KeystorePath) {
    $KeystorePath = Join-Path $secretsDir "raphcare-upload.keystore"
}

$keytool = Get-Command keytool -ErrorAction SilentlyContinue
if (-not $keytool) {
    $candidates = @(
        "$env:ProgramFiles\Android\Android Studio\jbr\bin\keytool.exe",
        "$env:ProgramFiles\Java\*\bin\keytool.exe",
        "$env:LOCALAPPDATA\Programs\Android\Android Studio\jbr\bin\keytool.exe"
    )
    foreach ($pattern in $candidates) {
        $hit = Get-Item $pattern -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($hit) { $keytool = $hit.FullName; break }
    }
}
if (-not $keytool) {
    throw "keytool not found. Install a JDK or Android Studio JBR and ensure keytool is on PATH."
}

if (Test-Path $KeystorePath) {
    Write-Warning "Keystore already exists at $KeystorePath. Not overwriting."
}
else {
    & $keytool -genkeypair -v `
        -keystore $KeystorePath `
        -alias $Alias `
        -keyalg RSA `
        -keysize 2048 `
        -validity $ValidityDays `
        -storepass $KeystorePassword `
        -keypass $KeyPassword `
        -dname $DistinguishedName
    if ($LASTEXITCODE -ne 0) { throw "keytool failed" }
}

$meta = [ordered]@{
    keystorePath = $KeystorePath
    alias        = $Alias
    createdUtc   = [DateTime]::UtcNow.ToString("o")
    note         = "Passwords are not stored. Keep them in a password manager."
}
$metaPath = Join-Path $secretsDir "keystore-meta.json"
$meta | ConvertTo-Json | Set-Content $metaPath -Encoding UTF8

Write-Host "Keystore: $KeystorePath"
Write-Host "Alias: $Alias"
Write-Host "Meta: $metaPath"
Write-Host "Back up the keystore offline. Losing it blocks Play updates signed with this upload key."
