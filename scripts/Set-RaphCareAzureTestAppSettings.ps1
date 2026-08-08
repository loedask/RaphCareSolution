<#
.SYNOPSIS
  Sets App Service application settings for the RaphCare test API from artifacts or parameters.
#>
[CmdletBinding()]
param(
    [string] $EnvironmentJsonPath = "",
    [Parameter(Mandatory = $true)]
    [string] $TenantId,
    [Parameter(Mandatory = $true)]
    [string] $ApiAppClientId,
    [string] $Audience = "api://raphcare-api",
    [Parameter(Mandatory = $true)]
    [string] $JwtSecret,
    [string] $WebPortalBaseUrl = "",
    [string] $SmtpPassword = "",
    [string] $SmtpHost = "mail.yindula.com",
    [string] $SmtpUsername = "raphcare@yindula.com",
    [string] $FromAddress = "raphcare@yindula.com"
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
if (-not $EnvironmentJsonPath) {
    $EnvironmentJsonPath = Join-Path $repoRoot "artifacts\azure-test-environment.json"
}
if (-not (Test-Path $EnvironmentJsonPath)) {
    throw "Missing $EnvironmentJsonPath. Run New-RaphCareAzureTestEnvironment.ps1 first."
}

az account show -o none 2>$null
if ($LASTEXITCODE -ne 0) { throw "Azure CLI is not logged in. Run: az login" }

$envInfo = Get-Content $EnvironmentJsonPath -Raw | ConvertFrom-Json
if (-not $WebPortalBaseUrl) { $WebPortalBaseUrl = $envInfo.webPortalBaseUrl }

$authority = "https://login.microsoftonline.com/$TenantId/v2.0"
$settings = @(
    "ASPNETCORE_ENVIRONMENT=Staging"
    "ConnectionStrings__DefaultConnection=$($envInfo.connectionString)"
    "Entra__TenantId=$TenantId"
    "Entra__Authority=$authority"
    "Entra__ClientId=$ApiAppClientId"
    "Entra__Audience=$Audience"
    "Jwt__Secret=$JwtSecret"
    "Jwt__Issuer=RaphCare"
    "Jwt__Audience=RaphCare.Mobile"
    "RaphCare__WebPortalBaseUrl=$WebPortalBaseUrl"
    "Cors__WebAdminOrigins__0=$WebPortalBaseUrl"
    "Smtp__Host=$SmtpHost"
    "Smtp__Port=465"
    "Smtp__UseSsl=true"
    "Smtp__Username=$SmtpUsername"
    "Smtp__FromAddress=$FromAddress"
    "Smtp__FromDisplayName=RaphCare"
)

if ($SmtpPassword) {
    $settings += "Smtp__Password=$SmtpPassword"
}

Write-Host "Updating App Service settings for $($envInfo.apiAppName) ..."
$azArgs = @(
    "webapp", "config", "appsettings", "set",
    "--resource-group", $envInfo.resourceGroup,
    "--name", $envInfo.apiAppName,
    "--settings"
) + $settings + @("-o", "none")
& az @azArgs
if ($LASTEXITCODE -ne 0) { throw "Failed to set App Service settings" }

$envInfo | Add-Member -NotePropertyName entraTenantId -NotePropertyValue $TenantId -Force
$envInfo | Add-Member -NotePropertyName apiAppClientId -NotePropertyValue $ApiAppClientId -Force
$envInfo | Add-Member -NotePropertyName webPortalBaseUrl -NotePropertyValue $WebPortalBaseUrl -Force
$envInfo | ConvertTo-Json -Depth 5 | Set-Content $EnvironmentJsonPath -Encoding UTF8

Write-Host "App settings applied. Run Update-RaphCareAzureSqlMigrations.ps1 next."
