# RaphCare Android test hosting (Azure + Play Internal)

Runbook to put the **patient Android app** and **hosted API / admin Web** online for remote testers. Local demo stays in [Mobile_Demo_Launch_Guide.md](./Mobile_Demo_Launch_Guide.md). Entra app registration detail stays in [Azure_Entra_Registration_Guide.md](./Azure_Entra_Registration_Guide.md).

## What you get

| Piece | Azure / Google resource |
|-------|-------------------------|
| Patient Android app | Google Play **Internal testing** track |
| API | App Service `raphcare-api` |
| Admin Web (Blazor WASM) | Linux App Service `raphcare` via **`RaphCare.Web.Host`** |
| Database | Azure SQL database |
| Private files | Azure Blob (`patient-photos`, `voice-recordings`) |
| Sign-in | Entra app registrations in your tenant (optional for phone OTP) |
| Ops mailbox | `raphcare@yindula.com` |
| Store mailbox | `apps@yindula.com` (Play Console / later App Store) |

Default resource group: **`raphcare_group`** (or `rg-raphcare-test` from scripts).

## Scripts (repo)

| Script | Purpose |
|--------|---------|
| [`scripts/Test-RaphCareMailbox.ps1`](../scripts/Test-RaphCareMailbox.ps1) | DNS / MX check for `yindula.com` and optional SMTP auth smoke |
| [`scripts/New-RaphCareEntraTestApps.ps1`](../scripts/New-RaphCareEntraTestApps.ps1) | Create Entra API / Mobile / Web app registrations |
| [`scripts/Invoke-RaphCareAndroidTestHosting.ps1`](../scripts/Invoke-RaphCareAndroidTestHosting.ps1) | One-shot: provision → Entra → settings → migrate → publish |
| [`scripts/Set-RaphCareAzureTestAppSettings.ps1`](../scripts/Set-RaphCareAzureTestAppSettings.ps1) | Push connection string, Entra, JWT, CORS, SMTP into App Service |
| [`scripts/Update-RaphCareAzureSqlMigrations.ps1`](../scripts/Update-RaphCareAzureSqlMigrations.ps1) | Apply all EF contexts to Azure SQL |
| [`scripts/Publish-RaphCareApiToAzure.ps1`](../scripts/Publish-RaphCareApiToAzure.ps1) | `dotnet publish` + zip deploy API |
| [`scripts/New-RaphCareAzureBlobStorage.ps1`](../scripts/New-RaphCareAzureBlobStorage.ps1) | Private Blob account for photos and voice files; sets `AzureStorage` on `raphcare-api` |
| [`scripts/Publish-RaphCareWebToAzure.ps1`](../scripts/Publish-RaphCareWebToAzure.ps1) | Publish **`RaphCare.Web.Host`** (linux-x64) to App Service `raphcare` |
| [`scripts/New-RaphCareAndroidUploadKeystore.ps1`](../scripts/New-RaphCareAndroidUploadKeystore.ps1) | Create upload keystore (gitignored path) |
| [`scripts/Publish-RaphCareAndroidPlay.ps1`](../scripts/Publish-RaphCareAndroidPlay.ps1) | Signed Release AAB for Play Internal |

---

## GitHub Actions (API + Web App Services)

App Service names in this setup:

| Azure App Service | Project | Workflow |
|-------------------|---------|----------|
| **`raphcare-api`** | `RaphCare.API` | [`.github/workflows/develop_raphcare-api.yml`](../.github/workflows/develop_raphcare-api.yml) |
| **`raphcare`** | `RaphCare.Web.Host` (serves WASM) | [`.github/workflows/develop_raphcare.yml`](../.github/workflows/develop_raphcare.yml) |

Both deploy from **`develop`**. Path filters skip Mobile. Use **Run workflow** on either job if you need a deploy without a matching path change.

OIDC identities in `raphcare_group`:

- `id-github-raphcare-api` (API)
- `id-github-raphcare-web` (Web)

Each has a federated credential for `repo:loedask/RaphCareSolution:ref:refs/heads/develop` and **Contributor** on the resource group.

### GitHub secrets / variables

In the GitHub repo → **Settings → Secrets and variables → Actions**:

| Name | Purpose |
|------|---------|
| `AZURE_TENANT_ID` | Shared Entra tenant id |
| `AZURE_SUBSCRIPTION_ID` | Shared subscription id |
| `AZURE_CLIENT_ID_RAPHCAREAPI` | Client id of `id-github-raphcare-api` |
| `AZURE_CLIENT_ID_RAPHCARE` | Client id of `id-github-raphcare-web` |
| `RAPHCARE_API_BASE_URL` (variable, optional) | Web `ApiBaseUrl`; default `https://raphcare-api-eydjcnefhae2dpa2.southafricanorth-01.azurewebsites.net` |
| `RAPHCARE_WEB_RESOURCE_GROUP` (variable, optional) | Resource group for the **raphcare** web app; default `raphcare_group` |

If you recreate the identities, update the two `AZURE_CLIENT_ID_*` secrets. Do not commit client secrets; GitHub Actions uses OIDC, not a password.

After first API deploy, set App Service **Configuration** on **raphcare-api** (SQL connection string, Entra, JWT, CORS, SMTP). Use these origins for direct app-to-API calls (no API Management):

- `RaphCare__WebPortalBaseUrl` = `https://raphcare-hqf6gsa3acanargz.southafricanorth-01.azurewebsites.net`
- `Cors__WebAdminOrigins__0` = `https://raphcare-hqf6gsa3acanargz.southafricanorth-01.azurewebsites.net`

On App Service **raphcare** (the admin web host, not the API), set:

- `ASPNETCORE_ENVIRONMENT` = `Staging`
- `ApiBaseUrl` = `https://raphcare-api-eydjcnefhae2dpa2.southafricanorth-01.azurewebsites.net`

Those two web settings are how the browser finds the API. SMTP stays on **raphcare-api**. Empty settings on **raphcare** leave the WASM client on `http://localhost:5281`.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Set-RaphCareAzureWebAppSettings.ps1
```

You still need a **RaphCare.Web.Host** deploy that includes the host forwarding code. Visual Studio Zip Deploy does not rewrite `wwwroot/appsettings.json` in source; the host reads `ApiBaseUrl` from App Service at runtime.

| App | URL |
|-----|-----|
| Web (`raphcare`) | `https://raphcare-hqf6gsa3acanargz.southafricanorth-01.azurewebsites.net` |
| API (`raphcare-api`) | `https://raphcare-api-eydjcnefhae2dpa2.southafricanorth-01.azurewebsites.net` |

---

## Phase 0: Mailboxes `raphcare@yindula.com` and `apps@yindula.com`

1. Confirm you control **yindula.com** DNS.
2. Create mailbox **`raphcare@yindula.com`** on `mail.yindula.com` (or Microsoft 365 with that domain). This is product / ops mail (SMTP, bug reports).
3. Create mailbox **`apps@yindula.com`** on the same host. This is the store identity (Play Console Google Account, public developer email, later Apple). Do not use `raphcare@` for the Play listing.
4. Prefer signing up Azure with `raphcare@yindula.com` as a **Microsoft work or personal account**.
5. Keep mailbox passwords out of git. Set `Smtp__Password` on App Service later.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Test-RaphCareMailbox.ps1
# Optional with credentials:
# ... -SmtpUser raphcare@yindula.com -SmtpPassword '<secret>'
```

---

## Phase 1: Azure login and environment

### Sign in (do this correctly)

Prefer interactive browser login. It works with MFA and Entra security defaults:

```powershell
az login
az account show
az account list -o table
```

You need a listed **subscription**. If the list is empty, that Microsoft account is not on the Azure subscription yet (grant Owner or Contributor in the portal), or security defaults blocked the tenant.

**Avoid this pattern** (it caused real failures on 2 Sep 2026):

```powershell
# Do not use for normal publish:
az login --tenant '<tenant-id>' --scope 'https://management.core.windows.net//.default' --use-device-code
```

That often ends as:

| Symptom | Likely cause |
|---------|----------------|
| Sign-in succeeded but **You don't have access to this** | Forced tenant + management scope on device code; wrong consent path |
| `AADSTS530035` (security defaults) | Tenant blocks the auth method until MFA / security defaults are sorted |
| `AADSTS50132` on `az webapp deploy` while `az account show` still works | Stale CLI session; run `az login` again (browser), then republish |
| `AADSTS70020` / device code expired | Code timed out; start a new `az login` |
| No subscriptions found for `raphcare@yindula.com` | Mailbox can authenticate but has no subscription role |

Device code (`az login --use-device-code`) is a fallback only when a browser cannot open. Prefer plain `az login` first. Agents cannot complete Microsoft MFA inside Cursor; the human must finish the browser prompt.

Ops account for this environment is often **`raphcare@yindula.com`**. Use the account that owns resource group **`raphcare_group`** / App Services **`raphcare-api`** and **`raphcare`**.

Cursor agents: see **`.cursor/rules/azure-cli-signin.mdc`**.

### Provision environment

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/New-RaphCareAzureTestEnvironment.ps1 `
  -Location southafricanorth `
  -SqlAdminUser raphcaresqladmin `
  -SqlAdminPassword '<StrongPasswordHere!>'
```

The script writes `artifacts/azure-test-environment.json` (gitignored) with hostnames and connection string template.

---

## Phase 2: Entra registrations

In the Entra tenant attached to this Azure subscription:

1. **RaphCare API**: Application ID URI `api://raphcare-api`, scope `access_as_user`, app roles `Administrator`, `Clinician`, `Patient`.
2. **RaphCare Mobile**: public client, redirect `msal{MOBILE_CLIENT_ID}://auth`, API permission to that scope, allow public client flows.
3. **RaphCare Web** (SPA): redirect to the static website origin from the environment JSON.
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

Set App Service **`ASPNETCORE_ENVIRONMENT=Staging`** on **raphcare-api**. Startup then runs `ApplyMigrationsAsync` (migrate + seed) automatically.

Staging also fills **RaphCare Demo Clinic** and these email/password accounts (no Microsoft sign-in, no extra email code):

| Role | Email |
|------|--------|
| Hospital admin | `demo.admin@raphcare.com` |
| Doctor | `demo.doctor@raphcare.com` |
| Pharmacist | `demo.pharmacy@raphcare.com` |
| Lab | `demo.lab@raphcare.com` |
| Patient (mobile) | `demo.patient@raphcare.com` |

Password: App Service setting **`Demo:Password`**. If that is empty, the seeder uses `RaphCareDemo!2026`. Rotate it on the App Service when you want a new password. The pack is additive. It does not delete Daskana or other hospitals.

You can still migrate from a PC that can reach Azure SQL:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Update-RaphCareAzureSqlMigrations.ps1
```

Production should keep **`ASPNETCORE_ENVIRONMENT=Production`** (no auto-migrate).

---

## Phase 4: Deploy API and Web

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Publish-RaphCareApiToAzure.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Publish-RaphCareWebToAzure.ps1
```

Smoke:

- Open `https://raphcare-api-eydjcnefhae2dpa2.southafricanorth-01.azurewebsites.net/` (expect no Swagger in Production).
- Open `https://raphcare-hqf6gsa3acanargz.southafricanorth-01.azurewebsites.net/`; admin UI should call the API (CORS uses `RaphCare:WebPortalBaseUrl` / `Cors:WebAdminOrigins`).
- Sign in with an Entra user that has the right app role, or exercise phone OTP if Twilio is configured.

### Code notes already in the repo

- API CORS reads `Cors:WebAdminOrigins` and `RaphCare:WebPortalBaseUrl` ([`Program.cs`](../RaphCare.API/Program.cs)).
- Mobile package id: **`com.yindula.raphcare`**.
- Release builds load [`appsettings.TestHosting.json`](../RaphCare.Mobile/appsettings.TestHosting.json) (API base URL). Publish scripts rewrite that URL to match your App Service hostname.
- **Linux Web publish:** use **`RaphCare.Web.Host`** (Zip Deploy / GitHub Actions), not standalone `RaphCare.Web`. Local admin UI can still `dotnet run` on `RaphCare.Web`.
- Hosted admin WASM: `ASPNETCORE_ENVIRONMENT=Staging` plus `ApiBaseUrl` on App Service **raphcare**. [`RaphCare.Web/wwwroot/appsettings.json`](../RaphCare.Web/wwwroot/appsettings.json) stays `localhost` for local runs. Staging overlay: [`appsettings.Staging.json`](../RaphCare.Web/wwwroot/appsettings.Staging.json).

---

## Phase 5: Android signing and Play Internal

RaphCare publishes under **Yindula** as a Play Console **Organization** account, not a personal developer account. RaphCare is a health app; Google expects organization accounts for that. Organization accounts also skip the closed-test gate that new personal accounts have (a long wait with many testers before production).

You cannot change Personal to Organization later. A wrong type means a new account, another **US$25** fee, and an app transfer.

A person at Yindula must finish identity checks, pay the fee, and upload company documents. Do not put government IDs, card numbers, or passwords in git or chat.

Google’s own pages: [account type](https://support.google.com/googleplay/android-developer/answer/13634885), [required information](https://support.google.com/googleplay/android-developer/answer/13628312), [get started](https://support.google.com/googleplay/android-developer/answer/6112435), [identity verification](https://support.google.com/googleplay/android-developer/answer/10841920).

### 5.1 Documents and field values (collect before any signup)

Use the **legal** company name and registered address on every form (D-U-N-S, Google Payments, Play Console, company documents). A trading nickname that does not match CIPC / incorporation papers will fail verification.

| Item | Use |
|------|-----|
| Legal company name and registered address | Must match D-U-N-S, Payments profile, and company docs |
| Company website (`yindula.com` or `raphcare.com`) | Google verifies the site (often via Search Console) |
| Mailbox **`apps@yindula.com`** | Google Account, Play contact email, and public developer email (Phase 0) |
| Company phone | Payments profile and Play contact |
| Government ID of the person registering (director or authorized officer) | Identity verification |
| Company registration document (CIPC, certificate of incorporation, or VAT, as applicable) | Organization verification |
| Card for the one-time **US$25** Play registration fee | Play Console signup |
| **D-U-N-S number** (nine digits from Dun & Bradstreet) | Mandatory for organization Play accounts |

Suggested Play fields once those match:

| Play field | Suggested value |
|------------|-----------------|
| Developer name (public on Play) | Yindula Technologies (or **Yindula**) |
| Organization legal name | YINDULA TECHNOLOGIES (PTY) LTD |
| CIPC enterprise number | `2026/322140/07` |
| Contact / developer email | `apps@yindula.com` |
| Package name (later, when creating the app) | `com.yindula.raphcare` |

Do not start Play Console **organization** signup until the D-U-N-S number is issued.

### 5.2 D-U-N-S (longest wait)

A [D-U-N-S number](https://www.dnb.com/duns-number.html) is a free nine-digit business id. Google will not create an organization Play account without one. Requesting it can take **up to 30 days**.

1. Search Dun & Bradstreet / TransUnion for **YINDULA TECHNOLOGIES (PTY) LTD**, CIPC **`2026/322140/07`**, country South Africa. Many companies already have a number from other vendors or banks. Use that number if the legal name and address match what you will type into Google.
2. If nothing matches, request a **free** D-U-N-S from TransUnion (`dnb@transunion.co.za`). Attach the CIPC certificate in that email only. Enter the same legal name, registered address, phone, and website you will use on Play.
3. Wait for the number. Keep a copy with the company records (not in this repo).
4. If Dun & Bradstreet asks which entity, pick the legal company that will own the app, not a trading name or a different subsidiary.

Government-only exceptions exist in Google’s help; Yindula is not in that path.

### 5.3 Google Account on `apps@yindula.com`

Play Console sits on a Google Account. Use **`apps@yindula.com`**, not a personal `@gmail.com` and not `raphcare@yindula.com`. If that mailbox is already a Google Workspace user, sign in with that Workspace account and skip signup.

1. Confirm you can read mail at **`apps@yindula.com`** (Phase 0).
2. Open [Google Account signup](https://accounts.google.com/signup).
3. Choose **Use my current email address**. Do not create a new `@gmail.com`.
4. Enter `apps@yindula.com`, set a strong password, and complete the mailbox verification code.
5. Turn on **2-step verification**. Prefer a company-controlled phone or a hardware key. Do not leave recovery only on one person’s personal phone.
6. Store the password and recovery codes with the company, outside git.

If Google says the address is already in use, that mailbox is already a Google Account or Workspace user. Sign in with it instead of creating a second identity.

### 5.4 Play Console as Organization

1. Sign in with the company Google Account at [Google Play Console](https://play.google.com/console).
2. Pay the one-time **US$25** registration fee (credit or debit card Google accepts).
3. Choose **Organization**, not Personal.
4. Create or select a **Google Payments** profile for the organization. Legal name, address, and D-U-N-S must match Dun & Bradstreet and the company document you will upload.
5. Fill organization details: legal name **Yindula Technologies (Pty) Ltd**, address, phone, website, contact name, contact email (`apps@yindula.com`), developer name (public: **Yindula** or **Yindula Technologies**).
6. Complete verification: government ID of the registrant, official organization document, website verification. Google emails if name or address does not match the document. Fix the Payments profile or re-upload; do not invent a second legal name.
7. Wait for Google’s approval email before creating the production-facing listing. You can still build the signed AAB locally while you wait (next subsection).

### 5.5 Upload keystore and signed AAB

Create the upload keystore (store path and passwords outside git):

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/New-RaphCareAndroidUploadKeystore.ps1 `
  -KeystorePassword '<secret>' `
  -KeyPassword '<secret>'
```

Versions live in `RaphCare.Mobile.csproj`:

- **ApplicationDisplayVersion** (for example `1.0.0`): what Settings shows, and the marketing part of the file name
- **ApplicationVersion** (integer, for example `2`): Android `versionCode`. Raise this for every Play upload, and for each sideload you want partners to tell apart

Artifact names look like `RaphCare-v1.0.0+2.aab` / `RaphCare-v1.0.0+2.apk`. A copy named `RaphCare-latest.apk` is written for convenience; prefer the versioned file when you send a build to someone.

Build the signed AAB (updates TestHosting API URL from `artifacts/azure-test-environment.json` when present):

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Publish-RaphCareAndroidPlay.ps1 `
  -KeystorePassword '<secret>' `
  -KeyPassword '<secret>' `
  -BumpBuild
```

Omit `-BumpBuild` if you already raised `ApplicationVersion` by hand.

### 5.6 Sideload APK (partner / device install)

Debug-signed Release APK pointed at the staging API (no Play keystore):

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Publish-RaphCareAndroidSideload.ps1 -BumpBuild
```

Send the versioned file under `artifacts/android/` (for example `RaphCare-v1.0.0+2.apk`). Uninstall any older RaphCare install first if Android refuses the update.

### 5.7 Create the app and Internal testing

After the organization account is approved:

1. Play Console → create app **RaphCare** → package **`com.yindula.raphcare`**.
2. **Testing → Internal testing** → upload the versioned AAB under `artifacts/android/`.
3. Add tester Gmail accounts → copy the opt-in link.

Optional while Play setup finishes: sideload the versioned APK produced next to the AAB.

---

## Phase 6: Tester handoff

Send testers:

- Play Internal opt-in link
- What to try (sign-in, appointments, care, devices)
- Known gaps (iOS not in this wave; push needs Firebase later; SMS OTP needs Twilio)
- AI assistant and AI discharge drafts need Azure OpenAI keys on **raphcare-api** (see [16_Azure_OpenAI_Setup.md](./16_Azure_OpenAI_Setup.md))
- Bug reports to **raphcare@yindula.com**

---

## Security

- Do not commit `artifacts/`, keystores, or real JWT / SQL / SMTP passwords.
- Do not commit D-U-N-S numbers, Play Console credentials, government IDs, or payment details. Keep those with the company.
- Replace sample `Jwt:Secret` values before any real patient data.
- Prefer App Service configuration (or Key Vault) over checking secrets into `appsettings.json`.
- Azure OpenAI keys belong in App Service settings or local User Secrets, not in git.
