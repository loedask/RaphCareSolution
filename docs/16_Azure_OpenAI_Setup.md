# Azure OpenAI setup (RaphCare)

This guide is for operators who need **real** patient assistant replies and **AI discharge drafts**. Voice onboarding does not use this resource. It stays on local Whisper unless you switch speech-to-text separately.

**Where configuration lives:** `RaphCare.API` (`PatientAssistant` in `appsettings.json`, User Secrets, or Azure App Service settings). The mobile app never stores the Azure OpenAI key. It calls the API.

**Cost model:** pay-as-you-go tokens only. Creating the resource and a standard `gpt-4.1-mini` deployment does not bill until someone chats or drafts a discharge summary. Do not create provisioned throughput (PTU). That bills by the hour even when idle.

---

## 1. What uses Azure OpenAI

| Feature | When keys are set | When keys are missing |
|---------|-------------------|------------------------|
| Patient assistant chat on the phone | Model reply plus the medical disclaimer | Fixed placeholder reply |
| Draft discharge summary in admin | Model draft from stay reason and ward vitals | Drafting unavailable. Staff write the summary |
| `POST api/ai/summary` | Model summary | Same unavailable message as discharge |

Clinical text for discharge drafts is sent only to the Azure OpenAI resource you create, not to public ChatGPT.

---

## 2. Cheapest working setup

Use **gpt-4.1-mini** on **Global Standard** (pay-as-you-go). Azure blocked new **gpt-4o-mini** deployments (that model is deprecating). gpt-4.1-mini is the next cheapest GPT-4-class chat model and uses the same completions API.

| Setting | Value |
|---------|--------|
| Azure account kind | OpenAI, sku S0 |
| Region | `southafricanorth` (same group as `raphcare-api`) |
| Deployment name | `gpt-4.1-mini` |
| Model version | `2025-04-14` |
| Deployment SKU | `GlobalStandard` (not PTU) |
| Capacity | `10` (falls back to `1` if quota is low) |

Global Standard is the SKU Azure lists for this model in South Africa North. Tokens are billed per million. Mini input and output rates are much lower than full GPT-4.1 or GPT-4o.

Cap blast radius:

- Keep `PatientAssistant:MaxCompletionTokens` at 512 (already the API default).
- Keep `PatientAssistant:MaxPriorMessages` at 8 (last few chat turns only; server windows again even if the phone sends more).
- Keep deployment capacity low (10K tokens per minute at capacity 10).
- Optional: a Cost Management budget on **this OpenAI account only** (default $15 / month, email `raphcare@yindula.com`). That is an alert, not a hard shutoff.

---

## 3. One-shot provision (staging)

Sign in first if needed:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Login-RaphCareAzure.ps1
```

Create the resource, deploy gpt-4.1-mini, set App Service settings on `raphcare-api`, restart the API, and try a tiny chat:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/New-RaphCareAzureOpenAi.ps1 -SmokeChat
```

Optional local User Secrets (so `dotnet run` on the API uses the same resource):

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/New-RaphCareAzureOpenAi.ps1 -ConfigureLocalUserSecrets
```

The script writes `artifacts/azure-openai.json` (endpoint and names, **no key**). `artifacts/` is gitignored.

---

## 4. Configuration keys (API)

Bind the `PatientAssistant` section.

| Key | Example | Description |
|-----|---------|-------------|
| `PatientAssistant:AzureOpenAiEndpoint` | `https://raphcare-openai.openai.azure.com/` | Resource endpoint |
| `PatientAssistant:AzureOpenAiApiKey` | `(secret)` | Key 1 from the resource |
| `PatientAssistant:AzureOpenAiDeployment` | `gpt-4.1-mini` | Deployment name (default) |
| `PatientAssistant:AzureOpenAiApiVersion` | `2024-08-01-preview` | Query string (default) |

**User Secrets** (do not commit real secrets):

```powershell
dotnet user-secrets set "PatientAssistant:AzureOpenAiEndpoint" "https://raphcare-openai.openai.azure.com/" --project RaphCare.API
dotnet user-secrets set "PatientAssistant:AzureOpenAiApiKey" "<key>" --project RaphCare.API
dotnet user-secrets set "PatientAssistant:AzureOpenAiDeployment" "gpt-4.1-mini" --project RaphCare.API
dotnet user-secrets set "PatientAssistant:AzureOpenAiApiVersion" "2024-08-01-preview" --project RaphCare.API
```

**App Service** (same hierarchy with `__`):

- `PatientAssistant__AzureOpenAiEndpoint`
- `PatientAssistant__AzureOpenAiApiKey`
- `PatientAssistant__AzureOpenAiDeployment`
- `PatientAssistant__AzureOpenAiApiVersion`

All three of endpoint, key, and deployment must be non-empty or `AIService` stays on placeholders.

---

## 5. Verify without printing the key

Settings present (local json plus User Secrets):

```powershell
pwsh tools/verify-azure-openai-config.ps1
```

Settings present on staging, then a tiny billed chat:

```powershell
pwsh tools/verify-azure-openai-config.ps1 -FromAzure -SmokeChat
```

---

## 6. What to try after it is wired

1. **Admin web:** on a hospital with the AI discharge toggle on, admit someone, add a vitals-only ward note, open discharge, tap Draft with AI. You should get editable draft text, not the "not connected" message.
2. **Patient app:** sign in, open the assistant chat, send a short wellness question. You should get a real reply plus the medical disclaimer. This is not a doctor. For urgent symptoms, contact a clinician or emergency services.

---

## 7. Troubleshooting

| Symptom | What to check |
|---------|----------------|
| Placeholder assistant reply | Endpoint, key, and deployment on the **API**, then restart `raphcare-api` |
| Discharge draft unavailable | Same keys, plus the hospital AI toggle under Organization details |
| 401 / 403 from Azure | Key rotated; copy key 1 again into App Service |
| 404 DeploymentNotFound | `AzureOpenAiDeployment` must match the deployment name (`gpt-4.1-mini`) |
| 429 RateLimitReached | Capacity is low by design. Wait and retry, or raise capacity in Foundry |
| Account create fails (name taken) | Rerun with `-AccountName raphcare-openai-<short-id>` |
| Account create fails (region) | Pass `-Location swedencentral` or `eastus` |

Do not log prompt bodies. `AIService` logs HTTP status and Azure error **code** only.

---

## 8. Speech-to-text is separate

`SpeechToText:Provider` defaults to Whisper on the API host. Azure Speech keys are optional and are a second bill. Leave Whisper unless you have a reason to switch.
