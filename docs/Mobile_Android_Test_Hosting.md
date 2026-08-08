# RaphCare Android test hosting (Azure + Play Internal)

Runbook to put the **patient Android app** and **hosted API / admin Web** online for remote testers. Local demo stays in [Mobile_Demo_Launch_Guide.md](./Mobile_Demo_Launch_Guide.md). Entra app registration detail stays in [Azure_Entra_Registration_Guide.md](./Azure_Entra_Registration_Guide.md).

## What you get

| Piece | Azure / Google resource |
|-------|-------------------------|
| Patient Android app | Google Play **Internal testing** track |
| API | App Service `raphcare-api-test` (default name) |
| Admin Web (Blazor WASM) | Storage static website |
| Database | Azure SQL database `raphcare` |
| Sign-in | Entra app registrations in your tenant |
| Ops mailbox | `raphcare@yindula.com` |

Default resource group: **`rg-raphcare-test`**.

## Scripts (repo)

| Script | Purpose |
|--------|---------|
| [`scripts/Test-RaphCareMailbox.ps1`](../scripts/Test-RaphCareMailbox.ps1) | DNS / MX check for `yindula.com` and optional SMTP auth smoke |
| [`scripts/New-RaphCareEntraTestApps.ps1`](../scripts/New-RaphCareEntraTestApps.ps1) | Create Entra API / Mobile / Web app registrations |
| [`scripts/Invoke-RaphCareAndroidTestHosting.ps1`](../scripts/Invoke-RaphCareAndroidTestHosting.ps1) | One-shot: provision → Entra → settings → migrate → publish |
| [`scripts/Set-RaphCareAzureTestAppSettings.ps1`](../scripts/Set-RaphCareAzureTestAppSettings.ps1) | Push connection string, Entra, JWT, CORS, SMTP into App Service |
| [`scripts/Update-RaphCareAzureSqlMigrations.ps1`](../scripts/Update-RaphCareAzureSqlMigrations.ps1) | Apply all EF contexts to Azure SQL |
| [`scripts/Publish-RaphCareApiToAzure.ps1`](../scripts/Publish-RaphCareApiToAzure.ps1) | `dotnet publish` + zip deploy API |
| [`scripts/Publish-RaphCareWebToAzure.ps1`](../scripts/Publish-RaphCareWebToAzure.ps1) | Publish Blazor WASM to `$web` container |
| [`scripts/New-RaphCareAndroidUploadKeystore.ps1`](../scripts/New-RaphCareAndroidUploadKeystore.ps1) | Create upload keystore (gitignored path) |
| [`scripts/Publish-RaphCareAndroidPlay.ps1`](../scripts/Publish-RaphCareAndroidPlay.ps1) | Signed Release AAB for Play Internal |

---

## GitHub Actions (API + Web App Services)

App Service names in this setup:

| Azure App Service | Project | Workflow |
|-------------------|---------|----------|
| **`raphcareapi`** | `RaphCare.API` | [`.github/workflows/develop_raphcare-api.yml`](../.github/workflows/develop_raphcare-api.yml) |
| **`raphcare`** | `RaphCare.Web` (Blazor WASM `wwwroot`) | [`.github/workflows/develop_raphcare.yml`](../.github/workflows/develop_raphcare.yml) |

Both deploy from **`develop`**. Path filters avoid rebuilding Mobile on every push.

### GitHub secrets / variables

In the GitHub repo → **Settings → Secrets and variables → Actions**:

| Name | Purpose |
|------|---------|
| `AZURE_TENANT_ID` | Shared Entra tenant id |
| `AZURE_SUBSCRIPTION_ID` | Shared subscription id |
| `AZURE_CLIENT_ID_RAPHCAREAPI` | User-assigned identity client id for **raphcareapi** |
| `AZURE_CLIENT_ID_RAPHCARE` | User-assigned identity client id for **raphcare** |
| `RAPHCARE_API_BASE_URL` (variable, optional) | Web `ApiBaseUrl`; default `https://raphcareapi.azurewebsites.net` |

Wire each App Service identity for GitHub OIDC (Deployment Center user-assigned identity, or federated credential on the managed identity). If Azure already created secret names like `AZUREAPPSERVICE_CLIENTID_...`, either rename them to match the table or edit the workflow `secrets.*` keys to match Azure’s names.

After first API deploy, set App Service **Configuration** on **raphcareapi** (SQL connection string, Entra, JWT, CORS including the **raphcare** origin).

---

## Phase 0: Mailbox `raphcare@yindula.com`

1. Confirm you control **yindula.com** DNS.
2. Create mailbox **`raphcare@yindula.com`** on `mail.yindula.com` (or Microsoft 365 with that domain).
3. Prefer signing up Azure with that address as a **Microsoft work or personal account**.
4. Keep the SMTP password out of git. Set `Smtp__Password` on App Service later.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Test-RaphCareMailbox.ps1
# Optional with credentials:
# ... -SmtpUser raphcare@yindula.com -SmtpPassword '<secret>'
```

---

## Phase 1: Azure login and environment

```powershell
az login
# or: az login --use-device-code

powershell -NoProfile -ExecutionPolicy Bypass -File scripts/New-RaphCareAzureTestEnvironment.ps1 `
  -Location southafricanorth `
  -SqlAdminUser raphcaresqladmin `
  -SqlAdminPassword '<StrongPasswordHere!>'
```

The script writes `artifacts/azure-test-environment.json` (gitignored) with hostnames and connection string template.

---

## Phase 2: Entra registrations

In the Entra tenant attached to this Azure subscription:

1. **RaphCare API** — Application ID URI `api://raphcare-api`, scope `access_as_user`, app roles `Administrator`, `Clinician`, `Patient`.
2. **RaphCare Mobile** — public client, redirect `msal{MOBILE_CLIENT_ID}://auth`, API permission to that scope, allow public client flows.
3. **RaphCare Web** (SPA) — redirect to the static website origin from the environment JSON.
4. Assign test users to app roles.

Then:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Set-RaphCareAzureTestAppSettings.ps1 `
  -TenantId '<tenant-guid>' `
  -ApiAppClientId '<api-app-client-id>' `
  -JwtSecret '<long-random-secret>' `
  -WebPortalBaseUrl 'https://<storage-account>.z13.web.core.windows.net'
```

---

## Phase 3: Database migrations

Production does **not** auto-migrate. From a machine that can reach Azure SQL (firewall rule for your IP is added by the provision script):

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Update-RaphCareAzureSqlMigrations.ps1
```

---

## Phase 4: Deploy API and Web

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Publish-RaphCareApiToAzure.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Publish-RaphCareWebToAzure.ps1
```

Smoke:

- Open `https://<api-app>.azurewebsites.net/` (expect no Swagger in Production).
- Open the static Web URL; admin UI should call the API (CORS uses `RaphCare:WebPortalBaseUrl` / `Cors:WebAdminOrigins`).
- Sign in with an Entra user that has the right app role, or exercise phone OTP if Twilio is configured.

### Code notes already in the repo

- API CORS reads `Cors:WebAdminOrigins` and `RaphCare:WebPortalBaseUrl` ([`Program.cs`](../RaphCare.API/Program.cs)).
- Mobile package id: **`com.yindula.raphcare`**.
- Release builds load [`appsettings.TestHosting.json`](../RaphCare.Mobile/appsettings.TestHosting.json) (API base URL). Publish scripts rewrite that URL to match your App Service hostname.

---

## Phase 5: Android signing and Play Internal

1. Create a [Google Play Console](https://play.google.com/console) developer account (one-time fee).
2. Create the upload keystore (store path and passwords outside git):

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/New-RaphCareAndroidUploadKeystore.ps1 `
  -KeystorePassword '<secret>' `
  -KeyPassword '<secret>'
```

3. Build the signed AAB (updates TestHosting API URL from `artifacts/azure-test-environment.json` when present):

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Publish-RaphCareAndroidPlay.ps1 `
  -KeystorePassword '<secret>' `
  -KeyPassword '<secret>'
```

4. Play Console → create app **RaphCare** → package **`com.yindula.raphcare`** → **Testing → Internal testing** → upload the AAB under `artifacts/android/` → add tester Gmail accounts → copy the opt-in link.

Optional while Play setup finishes: sideload the APK produced next to the AAB.

---

## Phase 6: Tester handoff

Send testers:

- Play Internal opt-in link
- What to try (sign-in, appointments, care, devices)
- Known gaps (iOS not in this wave; push needs Firebase later; SMS OTP needs Twilio)
- Bug reports to **raphcare@yindula.com**

---

## Security

- Do not commit `artifacts/`, keystores, or real JWT / SQL / SMTP passwords.
- Replace sample `Jwt:Secret` values before any real patient data.
- Prefer App Service configuration (or Key Vault) over checking secrets into `appsettings.json`.
