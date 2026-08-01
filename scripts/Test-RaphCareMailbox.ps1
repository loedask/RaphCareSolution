<#
.SYNOPSIS
  Checks DNS / MX for yindula.com and optionally authenticates SMTP for raphcare@yindula.com.
#>
[CmdletBinding()]
param(
    [string] $Domain = "yindula.com",
    [string] $Mailbox = "raphcare@yindula.com",
    [string] $SmtpHost = "mail.yindula.com",
    [int] $SmtpPort = 465,
    [string] $SmtpUser = "",
    [securestring] $SmtpPassword
)

$ErrorActionPreference = "Stop"

Write-Host "Mailbox checklist for $Mailbox"
Write-Host "1. Confirm you control DNS for $Domain."
Write-Host "2. Create the mailbox on $SmtpHost (or Microsoft 365)."
Write-Host "3. Use this address for Azure signup / billing contact when possible."
Write-Host "4. Store the SMTP password in App Service as Smtp__Password (never commit it)."
Write-Host ""

try {
    $mx = Resolve-DnsName -Name $Domain -Type MX -ErrorAction Stop
    Write-Host "MX records for ${Domain}:"
    $mx | ForEach-Object { Write-Host ("  {0} -> {1}" -f $_.Preference, $_.NameExchange) }
}
catch {
    Write-Warning "Could not resolve MX for $Domain. Create or fix DNS before relying on inbound mail. $_"
}

try {
    $a = Resolve-DnsName -Name $SmtpHost -Type A -ErrorAction Stop
    Write-Host "SMTP host $SmtpHost resolves:"
    $a | ForEach-Object { Write-Host ("  {0}" -f $_.IPAddress) }
}
catch {
    Write-Warning "Could not resolve $SmtpHost. $_"
}

if ($SmtpUser -and $SmtpPassword) {
    Write-Host "Attempting SMTP SSL auth to ${SmtpHost}:${SmtpPort} as $SmtpUser ..."
    $tcp = New-Object System.Net.Sockets.TcpClient
    $tcp.Connect($SmtpHost, $SmtpPort)
    $ssl = New-Object System.Net.Security.SslStream($tcp.GetStream(), $false, { $true })
    $ssl.AuthenticateAsClient($SmtpHost)
    $reader = New-Object System.IO.StreamReader($ssl)
    $writer = New-Object System.IO.StreamWriter($ssl)
    $writer.NewLine = "`r`n"
    $writer.AutoFlush = $true
    $banner = $reader.ReadLine()
    Write-Host "Server: $banner"
    $writer.WriteLine("EHLO raphcare-test")
    Start-Sleep -Milliseconds 300
    while ($ssl.CanRead -and $tcp.Available -gt 0) {
        Write-Host ("  " + $reader.ReadLine())
    }
    $b64User = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes($SmtpUser))
    $bstr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($SmtpPassword)
    try {
        $plain = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($bstr)
        $b64Pass = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes($plain))
    }
    finally {
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr)
    }
    $writer.WriteLine("AUTH LOGIN")
    $null = $reader.ReadLine()
    $writer.WriteLine($b64User)
    $null = $reader.ReadLine()
    $writer.WriteLine($b64Pass)
    $authResult = $reader.ReadLine()
    Write-Host "AUTH result: $authResult"
    $writer.WriteLine("QUIT")
    $ssl.Dispose()
    $tcp.Dispose()
    if ($authResult -notmatch '^235') {
        throw "SMTP authentication failed: $authResult"
    }
    Write-Host "SMTP authentication succeeded."
}
else {
    Write-Host "Skipped SMTP auth (pass -SmtpUser and -SmtpPassword to test)."
}

Write-Host ""
Write-Host "Phase 0 complete when the mailbox exists and you can sign into Azure with it (or a Microsoft account tied to it)."
