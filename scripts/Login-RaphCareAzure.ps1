<#
.SYNOPSIS
  Interactive Azure CLI login for RaphCare staging (raphcare_group).
.DESCRIPTION
  Use this when Cursor cannot open a browser, or when plain `az login` returns
  AADSTS50076 (MFA required) / no subscriptions for raphcare@yindula.com.

  Run in a visible Command Prompt or Windows Terminal, not as a hidden agent job.

  Do not add --use-device-code or --scope for the management resource. That path
  fails under Entra security defaults. See docs/Mobile_Android_Test_Hosting.md Phase 1.
#>
[CmdletBinding()]
param(
    [string] $TenantId = "6069ef19-5a1b-48ca-94ae-5b814804a78b"
)

$ErrorActionPreference = "Stop"

$az = "C:\Program Files\Microsoft SDKs\Azure\CLI2\wbin\az.cmd"
if (-not (Test-Path $az)) {
    $cmd = Get-Command az -ErrorAction SilentlyContinue
    if (-not $cmd) { throw "Azure CLI not found. Install Azure CLI, then retry." }
    $az = $cmd.Source
}

Write-Host "Signing in to tenant $TenantId (browser + MFA)."
Write-Host "Use the Microsoft account that can see subscription Azure subscription 1 / raphcare_group."
Write-Host ""

& $az login --tenant $TenantId
if ($LASTEXITCODE -ne 0) { throw "az login failed (exit $LASTEXITCODE)." }

Write-Host ""
Write-Host "Active account:"
& $az account show --query "{name:name, user:user.name, id:id, tenantId:tenantId}" -o table
Write-Host ""
& $az account list -o table
Write-Host ""
Write-Host "If the subscription list is empty, that user is not on the Azure subscription yet."
