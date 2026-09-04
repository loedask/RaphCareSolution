<#
.SYNOPSIS
  Provisions the RaphCare Azure test resource group: SQL, API App Service, storage static website.
.NOTES
  Requires: az CLI logged in with an active subscription.
#>
[CmdletBinding()]
param(
    [string] $ResourceGroup = "rg-raphcare-test",
    [string] $Location = "southafricanorth",
    [string] $SqlServerName = "",
    [string] $SqlDatabaseName = "raphcare",
    [Parameter(Mandatory = $true)]
    [string] $SqlAdminUser,
    [Parameter(Mandatory = $true)]
    [string] $SqlAdminPassword,
    [string] $ApiAppName = "",
    [string] $AppServicePlanName = "plan-raphcare-test",
    [string] $StorageAccountName = "",
    [string] $Sku = "B1"
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$artifactsDir = Join-Path $repoRoot "artifacts"
New-Item -ItemType Directory -Force -Path $artifactsDir | Out-Null

function Get-RandomDnsLabel {
    param([int] $Length = 8)
    -join ((97..122) | Get-Random -Count $Length | ForEach-Object { [char]$_ })
}

az account show -o none 2>$null
if ($LASTEXITCODE -ne 0) {
    throw "Azure CLI is not logged in. Run: az login"
}

$suffix = Get-RandomDnsLabel
if (-not $SqlServerName) { $SqlServerName = "sql-raphcare-$suffix" }
if (-not $ApiAppName) { $ApiAppName = "raphcare-api-test-$suffix" }
if (-not $StorageAccountName) {
    # Storage account names: 3-24 lowercase alphanumeric
    $StorageAccountName = ("raphcare" + $suffix).Substring(0, [Math]::Min(24, ("raphcare" + $suffix).Length))
}

$sqlPasswordPlain = $SqlAdminPassword

Write-Host "Creating resource group $ResourceGroup in $Location ..."
az group create --name $ResourceGroup --location $Location -o none

Write-Host "Creating SQL server $SqlServerName ..."
az sql server create `
    --name $SqlServerName `
    --resource-group $ResourceGroup `
    --location $Location `
    --admin-user $SqlAdminUser `
    --admin-password $sqlPasswordPlain `
    -o none

Write-Host "Allowing Azure services and current client IP on SQL firewall ..."
az sql server firewall-rule create `
    --resource-group $ResourceGroup `
    --server $SqlServerName `
    --name AllowAzureServices `
    --start-ip-address 0.0.0.0 `
    --end-ip-address 0.0.0.0 `
    -o none

try {
    $myIp = (Invoke-RestMethod -Uri "https://api.ipify.org" -TimeoutSec 15).Trim()
    if ($myIp) {
        az sql server firewall-rule create `
            --resource-group $ResourceGroup `
            --server $SqlServerName `
            --name AllowClientIp `
            --start-ip-address $myIp `
            --end-ip-address $myIp `
            -o none
        Write-Host "Allowed client IP $myIp"
    }
}
catch {
    Write-Warning "Could not detect public IP for SQL firewall. Add your IP in the portal before migrating. $_"
}

Write-Host "Creating database $SqlDatabaseName ..."
az sql db create `
    --resource-group $ResourceGroup `
    --server $SqlServerName `
    --name $SqlDatabaseName `
    --service-objective Basic `
    -o none

Write-Host "Creating App Service plan $AppServicePlanName ($Sku) ..."
az appservice plan create `
    --name $AppServicePlanName `
    --resource-group $ResourceGroup `
    --sku $Sku `
    --is-linux `
    -o none

Write-Host "Creating API web app $ApiAppName ..."
az webapp create `
    --resource-group $ResourceGroup `
    --plan $AppServicePlanName `
    --name $ApiAppName `
    --runtime "DOTNETCORE:10.0" `
    -o none

az webapp config set `
    --resource-group $ResourceGroup `
    --name $ApiAppName `
    --always-on true `
    -o none

az webapp config appsettings set `
    --resource-group $ResourceGroup `
    --name $ApiAppName `
    --settings `
        ASPNETCORE_ENVIRONMENT=Staging `
        WEBSITE_RUN_FROM_PACKAGE=1 `
    -o none

Write-Host "Creating storage account $StorageAccountName for admin Web ..."
az storage account create `
    --name $StorageAccountName `
    --resource-group $ResourceGroup `
    --location $Location `
    --sku Standard_LRS `
    --kind StorageV2 `
    --allow-blob-public-access true `
    -o none

az storage blob service-properties update `
    --account-name $StorageAccountName `
    --static-website `
    --index-document index.html `
    --404-document index.html `
    -o none

$webHost = az storage account show `
    --name $StorageAccountName `
    --resource-group $ResourceGroup `
    --query "primaryEndpoints.web" -o tsv
$webOrigin = $webHost.TrimEnd('/')

$sqlFqdn = "$SqlServerName.database.windows.net"
$connectionString = "Server=tcp:$sqlFqdn,1433;Initial Catalog=$SqlDatabaseName;Persist Security Info=False;User ID=$SqlAdminUser;Password=$sqlPasswordPlain;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
$apiBase = "https://$ApiAppName.azurewebsites.net"

$envInfo = [ordered]@{
    resourceGroup       = $ResourceGroup
    location            = $Location
    sqlServerName       = $SqlServerName
    sqlDatabaseName     = $SqlDatabaseName
    sqlAdminUser        = $SqlAdminUser
    sqlFqdn             = $sqlFqdn
    connectionString    = $connectionString
    apiAppName          = $ApiAppName
    apiBaseUrl          = $apiBase
    apiBaseUrlSlash     = "$apiBase/"
    appServicePlanName  = $AppServicePlanName
    storageAccountName  = $StorageAccountName
    webStaticHost       = $webHost
    webPortalBaseUrl    = $webOrigin
    createdUtc          = [DateTime]::UtcNow.ToString("o")
}

$outPath = Join-Path $artifactsDir "azure-test-environment.json"
$envInfo | ConvertTo-Json -Depth 5 | Set-Content -Path $outPath -Encoding UTF8

# Point TestHosting placeholders at this environment (hostnames only; commit only after review).
$mobileTestHosting = Join-Path $repoRoot "RaphCare.Mobile\appsettings.TestHosting.json"
$webTestHosting = Join-Path $repoRoot "RaphCare.Portal\wwwroot\appsettings.TestHosting.json"
$webStaging = Join-Path $repoRoot "RaphCare.Portal\wwwroot\appsettings.Staging.json"
$mobileJson = Get-Content $mobileTestHosting -Raw | ConvertFrom-Json
$mobileJson.Api.BaseAddress = "$apiBase/"
$mobileJson | ConvertTo-Json -Depth 5 | Set-Content $mobileTestHosting -Encoding UTF8
$webApiJson = @{ ApiBaseUrl = $apiBase } | ConvertTo-Json
Set-Content $webTestHosting -Value $webApiJson -Encoding UTF8
Set-Content $webStaging -Value $webApiJson -Encoding UTF8

Write-Host ""
Write-Host "Provisioned. Environment written to $outPath"
Write-Host "API:  $apiBase"
Write-Host "Web:  $webOrigin"
Write-Host "Next: register Entra apps, then Set-RaphCareAzureTestAppSettings.ps1, migrations, publish scripts."
Write-Host "See docs/Mobile_Android_Test_Hosting.md"
