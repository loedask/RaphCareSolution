<#
.SYNOPSIS
  Creates a zip with forward-slash entry paths so Linux App Service zip deploy can extract it.
  Compress-Archive on Windows stores backslashes, which Kudu rsync rejects (Invalid argument 22).
#>
function Compress-RaphCareUnixZip {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string] $SourceDir,
        [Parameter(Mandatory = $true)]
        [string] $ZipPath
    )

    $resolved = (Resolve-Path $SourceDir).Path.TrimEnd('\', '/')
    if (Test-Path $ZipPath) { Remove-Item $ZipPath -Force }

    Add-Type -AssemblyName System.IO.Compression
    Add-Type -AssemblyName System.IO.Compression.FileSystem

    $zip = [System.IO.Compression.ZipFile]::Open($ZipPath, [System.IO.Compression.ZipArchiveMode]::Create)
    try {
        Get-ChildItem -Path $resolved -Recurse -File | ForEach-Object {
            $relative = $_.FullName.Substring($resolved.Length).TrimStart('\', '/').Replace('\', '/')
            [void][System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile(
                $zip,
                $_.FullName,
                $relative,
                [System.IO.Compression.CompressionLevel]::Optimal)
        }
    }
    finally {
        $zip.Dispose()
    }
}
