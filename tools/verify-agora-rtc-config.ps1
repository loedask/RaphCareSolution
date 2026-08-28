# Verifies AgoraRtc API config is present and can mint an RTC token (server-side).
# Does not print AppId or AppCertificate values.
#
# Usage (repo root):
#   pwsh tools/verify-agora-rtc-config.ps1
#
# Optional:
#   pwsh tools/verify-agora-rtc-config.ps1 -AppsettingsPath RaphCare.API/appsettings.Development.json

param(
    [string]$AppsettingsPath = "RaphCare.API/appsettings.json"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

$settingsFile = Join-Path $root $AppsettingsPath
if (-not (Test-Path $settingsFile)) {
    Write-Error "Settings file not found: $settingsFile"
}

$json = Get-Content $settingsFile -Raw | ConvertFrom-Json
$appId = [string]$json.AgoraRtc.AppId
$cert = [string]$json.AgoraRtc.AppCertificate

$configured = -not [string]::IsNullOrWhiteSpace($appId) -and -not [string]::IsNullOrWhiteSpace($cert)
Write-Host "Settings: $AppsettingsPath"
Write-Host "AgoraRtc:AppId set: $(-not [string]::IsNullOrWhiteSpace($appId)) (len=$($appId.Length))"
Write-Host "AgoraRtc:AppCertificate set: $(-not [string]::IsNullOrWhiteSpace($cert)) (len=$($cert.Length))"
Write-Host "IsConfigured: $configured"

if (-not $configured) {
    Write-Host "FAIL: Set AgoraRtc:AppId and AgoraRtc:AppCertificate (User Secrets preferred). See docs/10_Agora_Twilio_Setup.md"
    exit 1
}

$tmp = Join-Path $env:TEMP ("raphcare-agora-verify-" + [guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory -Path $tmp | Out-Null
try {
    $infraProj = (Join-Path $root "RaphCare.Infrastructure\RaphCare.Infrastructure.csproj").Replace('\', '/')
    $settingsEscaped = $settingsFile.Replace('\', '\\')

    $csproj = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="10.0.0" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="10.0.0" />
    <PackageReference Include="Microsoft.Extensions.Options.ConfigurationExtensions" Version="10.0.0" />
    <ProjectReference Include="$infraProj" />
  </ItemGroup>
</Project>
"@
    Set-Content -Path (Join-Path $tmp "AgoraVerify.csproj") -Value $csproj

    $program = @"
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Infrastructure.Configuration;
using RaphCare.Infrastructure.Telehealth;

var config = new ConfigurationBuilder()
    .AddJsonFile(@"$settingsEscaped", optional: false)
    .Build();

var services = new ServiceCollection();
services.Configure<AgoraRtcOptions>(config.GetSection(AgoraRtcOptions.SectionName));
services.AddSingleton<ITelehealthRtcTokenGenerator, AgoraRtcTokenService>();
var sp = services.BuildServiceProvider();
var gen = sp.GetRequiredService<ITelehealthRtcTokenGenerator>();

Console.WriteLine($"TokenGenerator.IsConfigured={gen.IsConfigured}");
if (!gen.IsConfigured)
{
    Console.WriteLine("FAIL: AgoraRtcTokenService reports not configured.");
    return 1;
}

var channel = "raphcare-verify-" + Guid.NewGuid().ToString("N")[..8];
var token = gen.BuildRtcToken(channel, uid: 42, ttlSeconds: 600);
Console.WriteLine($"Token minted: {!string.IsNullOrWhiteSpace(token)} (len={token.Length})");
Console.WriteLine($"Channel sample: {channel}");
Console.WriteLine("PASS: Agora server token path OK. Next: Android device join with Care & telehealth.");
return 0;
"@
    Set-Content -Path (Join-Path $tmp "Program.cs") -Value $program

    Write-Host "Building token smoke..."
    Push-Location $tmp
    try {
        & dotnet run -c Release --nologo
        if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    }
    finally {
        Pop-Location
    }
}
finally {
    Remove-Item -Recurse -Force $tmp -ErrorAction SilentlyContinue
}
