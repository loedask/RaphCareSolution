# Verifies PatientAssistant Azure OpenAI settings without printing the API key.
#
# Usage (repo root):
#   pwsh tools/verify-azure-openai-config.ps1
#   pwsh tools/verify-azure-openai-config.ps1 -FromAzure
#   pwsh tools/verify-azure-openai-config.ps1 -FromAzure -SmokeChat
#
# -FromAzure reads App Service settings on raphcare-api (does not print the key).
# Default reads RaphCare.API/appsettings.json plus user secrets.

param(
    [string] $AppsettingsPath = "RaphCare.API/appsettings.json",
    [string] $ResourceGroup = "raphcare_group",
    [string] $ApiAppName = "raphcare-api",
    [switch] $FromAzure,
    [switch] $SmokeChat
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

function Get-SecretLength([string] $value) {
    if ([string]::IsNullOrWhiteSpace($value)) { return 0 }
    return $value.Trim().Length
}

$endpoint = ""
$key = ""
$deployment = ""
$apiVersion = "2024-08-01-preview"

if ($FromAzure) {
    az account show -o none 2>$null
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Azure CLI is not logged in. Run scripts/Login-RaphCareAzure.ps1"
    }

    $settings = az webapp config appsettings list --resource-group $ResourceGroup --name $ApiAppName -o json | ConvertFrom-Json
    $map = @{}
    foreach ($row in $settings) { $map[$row.name] = [string]$row.value }

    $endpoint = $map["PatientAssistant__AzureOpenAiEndpoint"]
    $key = $map["PatientAssistant__AzureOpenAiApiKey"]
    $deployment = $map["PatientAssistant__AzureOpenAiDeployment"]
    if ($map["PatientAssistant__AzureOpenAiApiVersion"]) {
        $apiVersion = $map["PatientAssistant__AzureOpenAiApiVersion"]
    }
    Write-Host "Source: App Service $ApiAppName"
}
else {
    $settingsFile = Join-Path $root $AppsettingsPath
    if (-not (Test-Path $settingsFile)) {
        Write-Error "Settings file not found: $settingsFile"
    }
    $json = Get-Content $settingsFile -Raw | ConvertFrom-Json
    $endpoint = [string]$json.PatientAssistant.AzureOpenAiEndpoint
    $key = [string]$json.PatientAssistant.AzureOpenAiApiKey
    $deployment = [string]$json.PatientAssistant.AzureOpenAiDeployment
    if ($json.PatientAssistant.AzureOpenAiApiVersion) {
        $apiVersion = [string]$json.PatientAssistant.AzureOpenAiApiVersion
    }

    $apiProj = Join-Path $root "RaphCare.API\RaphCare.API.csproj"
    $secretsRaw = dotnet user-secrets list --project $apiProj 2>$null
    if ($LASTEXITCODE -eq 0 -and $secretsRaw) {
        foreach ($line in $secretsRaw) {
            if ($line -match '^PatientAssistant:AzureOpenAiEndpoint\s*=\s*(.+)$') { $endpoint = $Matches[1].Trim() }
            if ($line -match '^PatientAssistant:AzureOpenAiApiKey\s*=\s*(.+)$') { $key = $Matches[1].Trim() }
            if ($line -match '^PatientAssistant:AzureOpenAiDeployment\s*=\s*(.+)$') { $deployment = $Matches[1].Trim() }
            if ($line -match '^PatientAssistant:AzureOpenAiApiVersion\s*=\s*(.+)$') { $apiVersion = $Matches[1].Trim() }
        }
    }
    Write-Host "Source: $AppsettingsPath plus user secrets (secrets win when set)"
}

$configured = (Get-SecretLength $endpoint) -gt 0 -and (Get-SecretLength $key) -gt 0 -and (Get-SecretLength $deployment) -gt 0
Write-Host "PatientAssistant:AzureOpenAiEndpoint set: $((Get-SecretLength $endpoint) -gt 0)"
Write-Host "PatientAssistant:AzureOpenAiApiKey set: $((Get-SecretLength $key) -gt 0) (len=$(Get-SecretLength $key))"
Write-Host "PatientAssistant:AzureOpenAiDeployment: $(if ($deployment) { $deployment } else { '(empty)' })"
Write-Host "PatientAssistant:AzureOpenAiApiVersion: $apiVersion"
Write-Host "IsConfigured: $configured"

if (-not $configured) {
    Write-Host "FAIL: Set PatientAssistant Azure OpenAI endpoint, key, and deployment. See docs/16_Azure_OpenAI_Setup.md"
    exit 1
}

if (-not $SmokeChat) {
    Write-Host "PASS: Azure OpenAI settings are present. Re-run with -SmokeChat to call chat completions."
    exit 0
}

$url = "$($endpoint.TrimEnd('/'))/openai/deployments/$([uri]::EscapeDataString($deployment))/chat/completions?api-version=$([uri]::EscapeDataString($apiVersion))"
$body = @{
    messages    = @(
        @{ role = "system"; content = "Reply with the single word ok." }
        @{ role = "user"; content = "ping" }
    )
    max_tokens  = 8
    temperature = 0
} | ConvertTo-Json -Depth 5

try {
    $chat = Invoke-RestMethod -Method Post -Uri $url -Headers @{ "api-key" = $key } -ContentType "application/json; charset=utf-8" -Body $body -TimeoutSec 45
    $text = [string]$chat.choices[0].message.content
    if ([string]::IsNullOrWhiteSpace($text)) {
        Write-Host "FAIL: Azure OpenAI returned an empty assistant message."
        exit 1
    }
    Write-Host "PASS: chat completions OK (reply length $($text.Trim().Length))."
    exit 0
}
catch {
    Write-Host "FAIL: chat completions call failed. $_"
    exit 1
}
