<#
.SYNOPSIS
  Creates a cheap Azure Blob account for private patient photos and voice recordings, then wires it to raphcare-api.
.DESCRIPTION
  Standard LRS Hot storage in the RaphCare resource group. Public blob access is off.
  Containers: patient-photos, voice-recordings.

  Requires: az CLI logged in. See scripts/Login-RaphCareAzure.ps1.
#>
[CmdletBinding()]
param(
    [string] $ResourceGroup = "raphcare_group",
    [string] $Location = "southafricanorth",
    [string] $AccountName = "raphcarefiles",
    [string] $Sku = "Standard_LRS",
    [string] $ApiAppName = "raphcare-api",
    [string] $PhotosContainer = "patient-photos",
    [string] $VoiceContainer = "voice-recordings",
    [switch] $SkipAppSettings
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$artifactsDir = Join-Path $repoRoot "artifacts"
New-Item -ItemType Directory -Force -Path $artifactsDir | Out-Null

az account show -o none 2>$null
if ($LASTEXITCODE -ne 0) {
    throw "Azure CLI is not logged in. Run scripts/Login-RaphCareAzure.ps1 in Windows Terminal."
}

Write-Host "Ensuring Microsoft.Storage is registered ..."
az provider register --namespace Microsoft.Storage --wait -o none
if ($LASTEXITCODE -ne 0) { throw "Could not register Microsoft.Storage." }

$existing = cmd /c "az storage account show --name $AccountName --resource-group $ResourceGroup -o json 2>nul"
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($existing)) {
    Write-Host "Creating storage account $AccountName in $Location ($Sku, public blob access off) ..."
    az storage account create `
        --name $AccountName `
        --resource-group $ResourceGroup `
        --location $Location `
        --sku $Sku `
        --kind StorageV2 `
        --access-tier Hot `
        --allow-blob-public-access false `
        --min-tls-version TLS1_2 `
        --https-only true `
        -o none
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to create storage account '$AccountName'. Names must be 3-24 lowercase letters and numbers. Rerun with -AccountName raphcarefiles<shortid>."
    }
}
else {
    Write-Host "Storage account $AccountName already exists. Reusing it."
}

$connectionString = cmd /c "az storage account show-connection-string --name $AccountName --resource-group $ResourceGroup --query connectionString -o tsv 2>nul"
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($connectionString)) {
    throw "Could not read the connection string for '$AccountName'."
}
$connectionString = [string]$connectionString.Trim()

foreach ($container in @($PhotosContainer, $VoiceContainer)) {
    Write-Host "Ensuring private container $container ..."
    az storage container create `
        --name $container `
        --connection-string $connectionString `
        --public-access off `
        -o none
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to create container '$container'."
    }
}

$envPath = Join-Path $artifactsDir "azure-test-environment.json"
if (Test-Path $envPath) {
    $envInfo = Get-Content $envPath -Raw | ConvertFrom-Json
    $envInfo | Add-Member -NotePropertyName storageAccountName -NotePropertyValue $AccountName -Force
    $envInfo | Add-Member -NotePropertyName storageConnectionString -NotePropertyValue $connectionString -Force
    $envInfo | ConvertTo-Json -Depth 6 | Set-Content $envPath -Encoding UTF8
    Write-Host "Wrote storageConnectionString into $envPath (gitignored)."
}

if (-not $SkipAppSettings) {
    Write-Host "Setting AzureStorage app settings on $ApiAppName ..."
    az webapp config appsettings set `
        --resource-group $ResourceGroup `
        --name $ApiAppName `
        --settings `
            "AzureStorage__ConnectionString=$connectionString" `
            "AzureStorage__PhotosContainer=$PhotosContainer" `
            "AzureStorage__VoiceRecordingsContainer=$VoiceContainer" `
        -o none
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to set App Service settings on '$ApiAppName'."
    }
}

Write-Host "Blob storage is ready. Redeploy or restart raphcare-api so it picks up AzureStorage__ConnectionString."
