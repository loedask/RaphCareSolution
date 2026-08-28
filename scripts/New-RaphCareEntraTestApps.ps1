<#
.SYNOPSIS
  Creates Entra app registrations for RaphCare API, Mobile, and Web (test hosting).
.NOTES
  Requires Azure CLI with permission to create app registrations in the signed-in tenant.
#>
[CmdletBinding()]
param(
    [string] $EnvironmentJsonPath = "",
    [string] $ApiDisplayName = "RaphCare API",
    [string] $MobileDisplayName = "RaphCare Mobile",
    [string] $WebDisplayName = "RaphCare Web",
    [string] $ApiAppIdUri = "api://raphcare-api"
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
if (-not $EnvironmentJsonPath) {
    $EnvironmentJsonPath = Join-Path $repoRoot "artifacts\azure-test-environment.json"
}

az account show -o none 2>$null
if ($LASTEXITCODE -ne 0) { throw "Azure CLI is not logged in. Run: az login" }

$tenantId = az account show --query tenantId -o tsv
$webRedirect = "https://localhost:7092"
if (Test-Path $EnvironmentJsonPath) {
    $envInfo = Get-Content $EnvironmentJsonPath -Raw | ConvertFrom-Json
    if ($envInfo.webPortalBaseUrl) { $webRedirect = $envInfo.webPortalBaseUrl.TrimEnd('/') }
}

Write-Host "Creating API app registration ..."
$apiAppJson = az ad app create `
    --display-name $ApiDisplayName `
    --sign-in-audience AzureADMyOrg `
    --identifier-uris $ApiAppIdUri `
    -o json
if ($LASTEXITCODE -ne 0) { throw "Failed to create API app registration" }
$apiApp = $apiAppJson | ConvertFrom-Json
$apiAppId = $apiApp.appId
$apiObjectId = $apiApp.id

# Expose access_as_user scope
$scopeId = [guid]::NewGuid().ToString()
$apiBody = @{
    oauth2PermissionScopes = @(
        @{
            adminConsentDescription = "Access RaphCare API as the signed-in user"
            adminConsentDisplayName = "Access RaphCare API"
            id                      = $scopeId
            isEnabled               = $true
            type                    = "User"
            userConsentDescription  = "Access RaphCare on your behalf"
            userConsentDisplayName  = "Access RaphCare"
            value                   = "access_as_user"
        }
    )
} | ConvertTo-Json -Depth 6 -Compress
az rest --method PATCH `
    --uri "https://graph.microsoft.com/v1.0/applications/$apiObjectId" `
    --headers "Content-Type=application/json" `
    --body $apiBody `
    -o none

# App roles
$roleDefs = @(
    @{ id = [guid]::NewGuid().ToString(); value = "Administrator"; description = "RaphCare administrator"; displayName = "Administrator" },
    @{ id = [guid]::NewGuid().ToString(); value = "Clinician"; description = "RaphCare clinician"; displayName = "Clinician" },
    @{ id = [guid]::NewGuid().ToString(); value = "Patient"; description = "RaphCare patient"; displayName = "Patient" }
)
$rolesPayload = @{
    appRoles = @(
        foreach ($r in $roleDefs) {
            @{
                allowedMemberTypes = @("User")
                description        = $r.description
                displayName        = $r.displayName
                id                 = $r.id
                isEnabled          = $true
                value              = $r.value
            }
        }
    )
} | ConvertTo-Json -Depth 6 -Compress
az rest --method PATCH `
    --uri "https://graph.microsoft.com/v1.0/applications/$apiObjectId" `
    --headers "Content-Type=application/json" `
    --body $rolesPayload `
    -o none

Write-Host "Creating Mobile app registration ..."
$mobileAppJson = az ad app create `
    --display-name $MobileDisplayName `
    --sign-in-audience AzureADMyOrg `
    --public-client-redirect-uris "msalplaceholder://auth" `
    --is-fallback-public-client true `
    -o json
if ($LASTEXITCODE -ne 0) { throw "Failed to create Mobile app registration" }
$mobileApp = $mobileAppJson | ConvertFrom-Json
$mobileAppId = $mobileApp.appId
$mobileObjectId = $mobileApp.id
$mobileRedirect = "msal${mobileAppId}://auth"

az ad app update --id $mobileAppId --public-client-redirect-uris $mobileRedirect -o none

# API permission for access_as_user
$permBody = @{
    requiredResourceAccess = @(
        @{
            resourceAppId  = $apiAppId
            resourceAccess = @(
                @{ id = $scopeId; type = "Scope" }
            )
        }
    )
} | ConvertTo-Json -Depth 6 -Compress
az rest --method PATCH `
    --uri "https://graph.microsoft.com/v1.0/applications/$mobileObjectId" `
    --headers "Content-Type=application/json" `
    --body $permBody `
    -o none

Write-Host "Creating Web SPA app registration ..."
$webAppJson = az ad app create `
    --display-name $WebDisplayName `
    --sign-in-audience AzureADMyOrg `
    --spa-redirect-uris $webRedirect "https://localhost:7092" `
    -o json
if ($LASTEXITCODE -ne 0) { throw "Failed to create Web app registration" }
$webApp = $webAppJson | ConvertFrom-Json
$webAppId = $webApp.appId

$result = [ordered]@{
    tenantId              = $tenantId
    apiAppClientId        = $apiAppId
    apiAppObjectId        = $apiObjectId
    apiAudience           = $ApiAppIdUri
    apiScope              = "$ApiAppIdUri/access_as_user"
    mobileAppClientId     = $mobileAppId
    mobileRedirectUri     = $mobileRedirect
    webAppClientId        = $webAppId
    webRedirectUri        = $webRedirect
    note                  = "Grant admin consent for Mobile→API permission in Entra portal if required. Assign users to API app roles."
}

$outDir = Join-Path $repoRoot "artifacts"
New-Item -ItemType Directory -Force -Path $outDir | Out-Null
$outPath = Join-Path $outDir "entra-test-apps.json"
$result | ConvertTo-Json -Depth 5 | Set-Content $outPath -Encoding UTF8

if (Test-Path $EnvironmentJsonPath) {
    $envInfo = Get-Content $EnvironmentJsonPath -Raw | ConvertFrom-Json
    $envInfo | Add-Member -NotePropertyName entraTenantId -NotePropertyValue $tenantId -Force
    $envInfo | Add-Member -NotePropertyName apiAppClientId -NotePropertyValue $apiAppId -Force
    $envInfo | Add-Member -NotePropertyName mobileAppClientId -NotePropertyValue $mobileAppId -Force
    $envInfo | Add-Member -NotePropertyName webAppClientId -NotePropertyValue $webAppId -Force
    $envInfo | ConvertTo-Json -Depth 5 | Set-Content $EnvironmentJsonPath -Encoding UTF8
}

Write-Host ""
Write-Host "Entra apps written to $outPath"
Write-Host "API client id:    $apiAppId"
Write-Host "Mobile client id: $mobileAppId"
Write-Host "Web client id:    $webAppId"
Write-Host "Next: Set-RaphCareAzureTestAppSettings.ps1 -TenantId $tenantId -ApiAppClientId $apiAppId -JwtSecret '<secret>'"
Write-Host "Update mobile Entra:ClientId to $mobileAppId for Release/TestHosting builds when ready."
