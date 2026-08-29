<#
.SYNOPSIS
  Reads or bumps RaphCare.Mobile ApplicationDisplayVersion / ApplicationVersion.
#>

function Get-RaphCareMobileCsprojPath {
    param([string] $RepoRoot)
    Join-Path $RepoRoot "RaphCare.Mobile\RaphCare.Mobile.csproj"
}

function Get-RaphCareMobileVersion {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string] $RepoRoot
    )

    $csproj = Get-RaphCareMobileCsprojPath -RepoRoot $RepoRoot
    if (-not (Test-Path $csproj)) {
        throw "Mobile project not found: $csproj"
    }

    $xml = [xml](Get-Content -Path $csproj -Raw)
    $ns = New-Object System.Xml.XmlNamespaceManager($xml.NameTable)
    # SDK-style projects usually have no xmlns on Project
    $display = $xml.SelectSingleNode("//ApplicationDisplayVersion")
    $code = $xml.SelectSingleNode("//ApplicationVersion")
    if (-not $display -or -not $code) {
        throw "ApplicationDisplayVersion / ApplicationVersion missing in $csproj"
    }

    [pscustomobject]@{
        CsprojPath     = $csproj
        DisplayVersion = $display.InnerText.Trim()
        VersionCode    = [int]$code.InnerText.Trim()
        Label          = "v$($display.InnerText.Trim())+$($code.InnerText.Trim())"
        FileStem       = "RaphCare-v$($display.InnerText.Trim())+$($code.InnerText.Trim())"
    }
}

function Set-RaphCareMobileVersionCode {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string] $RepoRoot,
        [Parameter(Mandatory = $true)]
        [int] $VersionCode
    )

    if ($VersionCode -lt 1) {
        throw "VersionCode must be >= 1"
    }

    $csproj = Get-RaphCareMobileCsprojPath -RepoRoot $RepoRoot
    $content = Get-Content -Path $csproj -Raw
    $updated = [regex]::Replace(
        $content,
        '(<ApplicationVersion>)\s*\d+\s*(</ApplicationVersion>)',
        "`${1}$VersionCode`${2}",
        1)
    if ($updated -eq $content) {
        throw "Could not update ApplicationVersion in $csproj"
    }
    Set-Content -Path $csproj -Value $updated -Encoding UTF8 -NoNewline
}

function Step-RaphCareMobileVersionCode {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string] $RepoRoot
    )

    $current = Get-RaphCareMobileVersion -RepoRoot $RepoRoot
    $next = $current.VersionCode + 1
    Set-RaphCareMobileVersionCode -RepoRoot $RepoRoot -VersionCode $next
    Get-RaphCareMobileVersion -RepoRoot $RepoRoot
}

function Copy-RaphCareAndroidArtifacts {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string] $ArtifactsDir,
        [Parameter(Mandatory = $true)]
        [string] $FileStem,
        [Parameter(Mandatory = $true)]
        [string[]] $SearchRoots
    )

    New-Item -ItemType Directory -Force -Path $ArtifactsDir | Out-Null
    $copied = @()
    $picked = @{}

    $candidates = foreach ($root in $SearchRoots) {
        if (-not (Test-Path $root)) { continue }
        Get-ChildItem -Path $root -Recurse -Include *.apk, *.aab -ErrorAction SilentlyContinue
    }

    foreach ($ext in @('.apk', '.aab')) {
        $matches = @($candidates | Where-Object { $_.Extension -eq $ext })
        if ($matches.Count -eq 0) { continue }

        $chosen = $matches | Where-Object { $_.Name -match 'Signed' } | Sort-Object LastWriteTime -Descending | Select-Object -First 1
        if (-not $chosen) {
            $chosen = $matches | Sort-Object LastWriteTime -Descending | Select-Object -First 1
        }

        $destName = "$FileStem$ext"
        $dest = Join-Path $ArtifactsDir $destName
        Copy-Item $chosen.FullName $dest -Force
        $copied += $dest
        $picked[$ext] = $dest
        Write-Host "Output: $dest"

        if ($ext -eq '.apk') {
            $latest = Join-Path $ArtifactsDir "RaphCare-latest.apk"
            Copy-Item $chosen.FullName $latest -Force
            Write-Host "Also:  $latest (same build, fixed name)"
        }
    }

    if ($copied.Count -eq 0) {
        throw "No .apk / .aab found under: $($SearchRoots -join ', ')"
    }

    $copied
}
