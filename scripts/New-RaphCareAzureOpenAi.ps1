<#
.SYNOPSIS
  Creates a pay-as-you-go Azure OpenAI account (gpt-4.1-mini Global Standard) and wires it to raphcare-api.
.DESCRIPTION
  Cheapest production path for patient assistant chat and AI discharge drafts.
  gpt-4o-mini is blocked for new Azure deployments (deprecating). This script uses
  gpt-4.1-mini Global Standard (pay-as-you-go). Does not create provisioned throughput (PTU).

  Requires: az CLI logged in. See docs/16_Azure_OpenAI_Setup.md and scripts/Login-RaphCareAzure.ps1.
#>
[CmdletBinding()]
param(
    [string] $ResourceGroup = "raphcare_group",
    [string] $Location = "southafricanorth",
    [string] $AccountName = "raphcare-openai",
    [string] $DeploymentName = "gpt-4.1-mini",
    [string] $ModelName = "gpt-4.1-mini",
    [string] $ModelVersion = "2025-04-14",
    [string] $SkuName = "GlobalStandard",
    [int] $Capacity = 10,
    [string] $ApiAppName = "raphcare-api",
    [string] $ApiVersion = "2024-08-01-preview",
    [decimal] $MonthlyBudgetUsd = 15,
    [string] $AlertEmail = "raphcare@yindula.com",
    [switch] $SkipAppSettings,
    [switch] $SkipBudget,
    [switch] $ConfigureLocalUserSecrets,
    [switch] $SmokeChat
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$artifactsDir = Join-Path $repoRoot "artifacts"
New-Item -ItemType Directory -Force -Path $artifactsDir | Out-Null

function Invoke-AzJson {
    param([Parameter(Mandatory = $true)][string[]] $AzArgs)
    $raw = cmd /c "az $($AzArgs -join ' ') -o json 2>nul"
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($raw)) {
        return $null
    }
    return $raw | ConvertFrom-Json
}

function Get-AzTsv {
    param([Parameter(Mandatory = $true)][string[]] $AzArgs)
    $raw = cmd /c "az $($AzArgs -join ' ') -o tsv 2>nul"
    if ($LASTEXITCODE -ne 0) { return "" }
    return [string]$raw
}

az account show -o none 2>$null
if ($LASTEXITCODE -ne 0) {
    throw "Azure CLI is not logged in. Run scripts/Login-RaphCareAzure.ps1 in Windows Terminal."
}

Write-Host "Ensuring Microsoft.CognitiveServices is registered ..."
az provider register --namespace Microsoft.CognitiveServices --wait -o none
if ($LASTEXITCODE -ne 0) { throw "Could not register Microsoft.CognitiveServices." }

$account = Invoke-AzJson @(
    "cognitiveservices", "account", "show",
    "--name", $AccountName,
    "--resource-group", $ResourceGroup
)

if (-not $account) {
    Write-Host "Creating Azure OpenAI account $AccountName in $Location (kind OpenAI, sku S0) ..."
    az cognitiveservices account create `
        --name $AccountName `
        --resource-group $ResourceGroup `
        --location $Location `
        --kind OpenAI `
        --sku S0 `
        --custom-domain $AccountName `
        --yes `
        -o none
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to create Azure OpenAI account '$AccountName' in '$Location'. If the name is taken, rerun with -AccountName raphcare-openai-<short-id>. If the region rejects OpenAI, pass -Location swedencentral."
    }
    $account = Invoke-AzJson @(
        "cognitiveservices", "account", "show",
        "--name", $AccountName,
        "--resource-group", $ResourceGroup
    )
}
else {
    Write-Host "Azure OpenAI account $AccountName already exists. Reusing it."
}

if (-not $account) { throw "Azure OpenAI account '$AccountName' was not found after create." }

$endpoint = [string]$account.properties.endpoint
if ([string]::IsNullOrWhiteSpace($endpoint)) {
    throw "Account exists but endpoint is empty."
}
$endpoint = $endpoint.TrimEnd('/')

$deployment = Invoke-AzJson @(
    "cognitiveservices", "account", "deployment", "show",
    "--name", $AccountName,
    "--resource-group", $ResourceGroup,
    "--deployment-name", $DeploymentName
)

if (-not $deployment) {
    Write-Host "Deploying $ModelName $ModelVersion as $DeploymentName ($SkuName capacity $Capacity) ..."
    az cognitiveservices account deployment create `
        --name $AccountName `
        --resource-group $ResourceGroup `
        --deployment-name $DeploymentName `
        --model-name $ModelName `
        --model-version $ModelVersion `
        --model-format OpenAI `
        --sku-capacity $Capacity `
        --sku-name $SkuName `
        -o none
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "Deployment of $ModelName $ModelVersion failed. Trying gpt-5-mini 2025-08-07 (Generally Available)."
        $DeploymentName = "gpt-5-mini"
        $ModelName = "gpt-5-mini"
        $ModelVersion = "2025-08-07"
        az cognitiveservices account deployment create `
            --name $AccountName `
            --resource-group $ResourceGroup `
            --deployment-name $DeploymentName `
            --model-name $ModelName `
            --model-version $ModelVersion `
            --model-format OpenAI `
            --sku-capacity $Capacity `
            --sku-name $SkuName `
            -o none
    }
    if ($LASTEXITCODE -ne 0 -and $Capacity -gt 1) {
        Write-Warning "Deployment at capacity $Capacity failed. Retrying with capacity 1 (subscription quota is often low on a new OpenAI resource)."
        az cognitiveservices account deployment create `
            --name $AccountName `
            --resource-group $ResourceGroup `
            --deployment-name $DeploymentName `
            --model-name $ModelName `
            --model-version $ModelVersion `
            --model-format OpenAI `
            --sku-capacity 1 `
            --sku-name $SkuName `
            -o none
        $Capacity = 1
    }
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to deploy $ModelName. Check quota in Azure AI Foundry for $Location, or try -Location eastus."
    }
}
else {
    Write-Host "Deployment $DeploymentName already exists. Reusing it."
}

$key = Get-AzTsv @(
    "cognitiveservices", "account", "keys", "list",
    "--name", $AccountName,
    "--resource-group", $ResourceGroup,
    "--query", "key1"
)
if ([string]::IsNullOrWhiteSpace($key)) {
    throw "Could not read Azure OpenAI key1."
}
$key = $key.Trim()

$subscriptionId = az account show --query id -o tsv
$resourceId = [string]$account.id

$info = [ordered]@{
    resourceGroup   = $ResourceGroup
    location        = $Location
    accountName     = $AccountName
    resourceId      = $resourceId
    endpoint        = $endpoint
    deploymentName  = $DeploymentName
    modelName       = $ModelName
    modelVersion    = $ModelVersion
    skuName         = $SkuName
    capacity        = $Capacity
    apiVersion      = $ApiVersion
    apiAppName      = $ApiAppName
    createdUtc      = [DateTime]::UtcNow.ToString("o")
}
$outPath = Join-Path $artifactsDir "azure-openai.json"
$info | ConvertTo-Json -Depth 5 | Set-Content -Path $outPath -Encoding UTF8
Write-Host "Wrote $outPath (no API key)."

if (-not $SkipAppSettings) {
    Write-Host "Setting PatientAssistant app settings on $ApiAppName ..."
    az webapp config appsettings set `
        --resource-group $ResourceGroup `
        --name $ApiAppName `
        --settings `
            "PatientAssistant__AzureOpenAiEndpoint=$endpoint" `
            "PatientAssistant__AzureOpenAiApiKey=$key" `
            "PatientAssistant__AzureOpenAiDeployment=$DeploymentName" `
            "PatientAssistant__AzureOpenAiApiVersion=$ApiVersion" `
        -o none
    if ($LASTEXITCODE -ne 0) { throw "Failed to set App Service settings on $ApiAppName." }

    az webapp restart --resource-group $ResourceGroup --name $ApiAppName -o none
    if ($LASTEXITCODE -ne 0) { Write-Warning "App settings were set but restart of $ApiAppName failed. Restart it in the portal." }
    else { Write-Host "Restarted $ApiAppName." }
}

if ($ConfigureLocalUserSecrets) {
    $apiProj = Join-Path $repoRoot "RaphCare.API\RaphCare.API.csproj"
    Write-Host "Writing User Secrets on RaphCare.API (not committed) ..."
    dotnet user-secrets set "PatientAssistant:AzureOpenAiEndpoint" $endpoint --project $apiProj | Out-Null
    dotnet user-secrets set "PatientAssistant:AzureOpenAiApiKey" $key --project $apiProj | Out-Null
    dotnet user-secrets set "PatientAssistant:AzureOpenAiDeployment" $DeploymentName --project $apiProj | Out-Null
    dotnet user-secrets set "PatientAssistant:AzureOpenAiApiVersion" $ApiVersion --project $apiProj | Out-Null
}

if (-not $SkipBudget) {
    Write-Host "Creating a `$$MonthlyBudgetUsd / month Cost Management budget on this OpenAI account only ..."
    $startDate = (Get-Date -Day 1).ToString("yyyy-MM-01T00:00:00Z")
    $endDate = (Get-Date -Day 1).AddYears(10).ToString("yyyy-MM-01T00:00:00Z")
    $budgetName = "raphcare-openai-monthly"
    $budgetBody = @{
        properties = @{
            category  = "Cost"
            amount    = $MonthlyBudgetUsd
            timeGrain = "Monthly"
            timePeriod = @{
                startDate = $startDate
                endDate   = $endDate
            }
            filter = @{
                dimensions = @{
                    name     = "ResourceId"
                    operator = "In"
                    values   = @($resourceId)
                }
            }
            notifications = @{
                Actual_GreaterThan_80_Percent = @{
                    enabled        = $true
                    operator       = "GreaterThan"
                    threshold      = 80
                    thresholdType  = "Actual"
                    contactEmails  = @($AlertEmail)
                }
                Actual_GreaterThan_100_Percent = @{
                    enabled        = $true
                    operator       = "GreaterThan"
                    threshold      = 100
                    thresholdType  = "Actual"
                    contactEmails  = @($AlertEmail)
                }
            }
        }
    } | ConvertTo-Json -Depth 8
    $budgetTemp = Join-Path $env:TEMP "raphcare-openai-budget.json"
    Set-Content -Path $budgetTemp -Value $budgetBody -Encoding UTF8
    $budgetUrl = "https://management.azure.com/subscriptions/$subscriptionId/resourceGroups/$ResourceGroup/providers/Microsoft.Consumption/budgets/${budgetName}?api-version=2023-11-01"
    az rest --method put --uri $budgetUrl --body "@$budgetTemp" -o none 2>$null
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "Budget was not created (Cost Management permission or API). Token billing still applies. Set a budget in the Azure portal on resource $AccountName if you want email alerts."
    }
    else {
        Write-Host "Budget $budgetName : alert $AlertEmail at 80% and 100% of `$$MonthlyBudgetUsd / month."
    }
}

if ($SmokeChat) {
    Write-Host "Sending a tiny chat completion (max 8 tokens) ..."
    $url = "$endpoint/openai/deployments/$([uri]::EscapeDataString($DeploymentName))/chat/completions?api-version=$([uri]::EscapeDataString($ApiVersion))"
    $chatBody = @{
        messages    = @(
            @{ role = "system"; content = "Reply with the single word ok." }
            @{ role = "user"; content = "ping" }
        )
        max_tokens  = 8
        temperature = 0
    } | ConvertTo-Json -Depth 5
    try {
        $chat = Invoke-RestMethod -Method Post -Uri $url -Headers @{ "api-key" = $key } -ContentType "application/json; charset=utf-8" -Body $chatBody -TimeoutSec 45
        $text = [string]$chat.choices[0].message.content
        if ([string]::IsNullOrWhiteSpace($text)) { throw "Empty assistant message." }
        Write-Host "Smoke chat OK (reply length $($text.Trim().Length))."
    }
    catch {
        throw "Smoke chat failed. Endpoint $endpoint deployment $DeploymentName. $_"
    }
}

Write-Host ""
Write-Host "Azure OpenAI is ready for RaphCare."
Write-Host "Endpoint:   $endpoint"
Write-Host "Deployment: $DeploymentName ($SkuName, pay-as-you-go)"
Write-Host "API app:    $ApiAppName"
Write-Host "Key:        set (not printed). Rotate in the portal if this log is shared."
Write-Host "See docs/16_Azure_OpenAI_Setup.md"
