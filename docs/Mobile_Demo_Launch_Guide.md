# RaphCare patient mobile — demo launch (settings checklist)

This doc is the **single runbook** to get **RaphCare.API** and **RaphCare.Mobile** running together for a **local demo**. Deeper topics stay in the linked guides.

| Topic | Where |
|--------|--------|
| Solution layout | [02_Solution_Structure.md](./02_Solution_Structure.md) |
| Entra (Azure) app registrations, API + mobile JSON | [Azure_Entra_Registration_Guide.md](./Azure_Entra_Registration_Guide.md) |
| Auth architecture, roles | [04_Authentication_Authorization.md](./04_Authentication_Authorization.md) |
| Phone OTP behavior | [06_Key_Workflows.md](./06_Key_Workflows.md) |
| Mobile project layout, flags, voice clinic | [09_Mobile_App_Guide.md](./09_Mobile_App_Guide.md) |
| Agora / Twilio (telehealth, SMS) | [10_Agora_Twilio_Setup.md](./10_Agora_Twilio_Setup.md) |
| Release / completeness bar | [Mobile_Release_Ready_Checklist.md](./Mobile_Release_Ready_Checklist.md) |

---

## 1. Prerequisites

- **.NET SDK** matching the repo (solution targets **.NET 10**).
- **MAUI workload** (for `RaphCare.Mobile`): `dotnet workload install maui` (or install via Visual Studio installer).
- **SQL Server** reachable from the API — sample `appsettings` use **LocalDB** (`(localdb)\mssqllocaldb`). Adjust connection strings if you use another instance.
- **Android SDK / emulator or device** (or **iOS** on a Mac with Xcode) if you are not only building **Windows** targets.

---

## 2. API base URL (must match the mobile app)

The API’s URLs come from **`RaphCare.API/Properties/launchSettings.json`** (default profiles):

| Profile | HTTPS | HTTP |
|--------|-------|------|
| **https** (typical F5) | `https://localhost:7146` | `http://localhost:5281` |
| **http** | — | `http://localhost:5281` |

**RaphCare.Mobile** must use the **same scheme, host, and port** you actually run, with a **trailing slash**, in **`Api:BaseAddress`** (see §5).

> Tracked **`RaphCare.Mobile/appsettings.json`** may still say `https://localhost:7001/` — that is only valid if your API is configured to listen on **7001**. For the stock **launchSettings** profile, override with User Secrets or edit locally (do not commit secrets).

---

## 3. API — required settings for a demo

### 3.1 Environment

- Run with **`ASPNETCORE_ENVIRONMENT=Development`** so:
  - **Swagger** is available.
  - **EF migrations** can apply on startup (see `Program.cs` / migration extension).
  - **SMS OTP** is logged to the console instead of sending real SMS (search the API log for **`[Development] SMS not sent. OTP for testing`** or **`Your RaphCare verification code is`**).

`launchSettings.json` already sets this for the **http** / **https** profiles.

### 3.2 Connection strings

In **`RaphCare.API/appsettings.json`**, **`ConnectionStrings`** must point at databases the process can create or migrate (default: multiple **RaphCare_*** databases on LocalDB). Change only if your machine does not use LocalDB.

### 3.3 JWT (API-issued tokens, e.g. phone OTP)

- **`Jwt`** section in **`RaphCare.API/appsettings.json`**: **`Secret`** must be a **strong, private** value for any shared or non-local demo. For **local only**, the sample placeholder may work; for **real data or shared networks**, use **User Secrets** or environment variables.

```bash
cd RaphCare.API
dotnet user-secrets set "Jwt:Secret" "<long-random-string>"
```

### 3.4 Microsoft Entra (required for **email / Entra** sign-in on mobile)

The API validates Bearer tokens from Entra. Configure **`Entra`** on the API per **[Azure_Entra_Registration_Guide.md](./Azure_Entra_Registration_Guide.md)** (Authority, TenantId, Audience, ClientId).

Use **User Secrets** for tenant-specific values:

```bash
dotnet user-secrets set "Entra:TenantId" "<tenant-guid>"
dotnet user-secrets set "Entra:Authority" "https://login.microsoftonline.com/<tenant-guid>/v2.0"
dotnet user-secrets set "Entra:Audience" "api://raphcare-api"
dotnet user-secrets set "Entra:ClientId" "<api-app-client-id>"
```

**Roles:** Users need **app roles** the API expects (**Patient**, **Clinician**, **Administrator**) or they will get **403** on protected endpoints. See the Entra guide and [04_Authentication_Authorization.md](./04_Authentication_Authorization.md).

### 3.5 Start the API

From the repo root (or API folder):

```bash
dotnet run --project RaphCare.API --launch-profile https
```

Note the listening URL in the console and set **`Api:BaseAddress`** on the mobile app to that base URL (§5).

---

## 4. Mobile — required settings for a demo

Configuration load order in **`MauiProgram`**: **`appsettings.json`** → **`appsettings.Development.json`** (DEBUG only) → **User Secrets**.

### 4.1 API address (required)

Set **`Api:BaseAddress`** to the running API (§2), including trailing slash:

```bash
cd RaphCare.Mobile
dotnet user-secrets set "Api:BaseAddress" "https://localhost:7146/"
```

If you use the **http** profile:

```bash
dotnet user-secrets set "Api:BaseAddress" "http://localhost:5281/"
```

### 4.2 Entra (required for **Sign in with Microsoft** / email path)

Must match the **RaphCare Mobile** app registration and the scope exposed by the API:

```bash
dotnet user-secrets set "Entra:ClientId" "<mobile-app-client-id>"
dotnet user-secrets set "Entra:TenantId" "<tenant-guid>"
dotnet user-secrets set "Entra:ApiScope" "api://raphcare-api/access_as_user"
```

Redirect URIs and platform-specific notes: **[Azure_Entra_Registration_Guide.md](./Azure_Entra_Registration_Guide.md)** §3 and §5.

### 4.3 Feature flags (what screens are enabled)

- **`appsettings.json`**: most vertical flags default to **`false`**; **`SettingsEnabled`** is **`true`**.
- **`appsettings.Development.json`** (DEBUG builds): all flags are **`true`** so you can navigate the full demo without extra secrets.

If you run **Release** or override config, enable the areas you need via User Secrets, for example:

```bash
dotnet user-secrets set "FeatureFlags:AppointmentsEnabled" "true"
```

See **`FeatureFlags`** in **`RaphCare.Mobile.Kernel`** and [09_Mobile_App_Guide.md](./09_Mobile_App_Guide.md).

### 4.4 HTTPS to localhost from Android emulator

If **`Api:BaseAddress`** uses **`https://localhost:...`**:

- The **Android emulator** maps **`10.0.2.2`** to the host loopback. You may need **`https://10.0.2.2:<port>/`** instead of **`localhost`** for the emulator to reach the API.
- The dev certificate must be **trusted** on the device, or use **HTTP** on a trusted network for local demo only.

Using **`http://10.0.2.2:5281/`** with the API **`http`** profile is often the simplest Android demo.

---

## 5. Optional settings (feature demos)

| Goal | Setting / doc |
|------|----------------|
| **Voice registration** after seed | **`Onboarding:VoiceRegistrationClinicId`** (clinic `Id` from DB) — [09_Mobile_App_Guide.md](./09_Mobile_App_Guide.md) |
| **Telehealth / Agora** (Android in-app video) | **`AgoraRtc`** on API — [10_Agora_Twilio_Setup.md](./10_Agora_Twilio_Setup.md) |
| **Real SMS** | **`Twilio`** on API when not using Development log OTP |
| **AI assistant (real model)** | **`PatientAssistant`** Azure OpenAI keys on API — see API **`appsettings.json`** / checklist |
| **Push notifications** | **`FirebasePush:ServiceAccountJsonPath`** on API — [Mobile_Release_Ready_Checklist.md](./Mobile_Release_Ready_Checklist.md) |
| **Appointments demo defaults** | **`Appointments:DefaultClinicId`** / **`DefaultProviderId`** in mobile **`appsettings`** (optional) |

None of these are required to **open the app**, sign in (Entra or phone OTP), and browse **Settings** and other enabled areas.

---

## 6. Quick start sequence

1. **Start SQL** (LocalDB or your server) so connection strings work.
2. **Configure API** User Secrets: **Entra**, optionally **Jwt:Secret**.
3. **Run API** (`https` or `http` profile); confirm Swagger or console URL.
4. **Configure Mobile** User Secrets: **`Api:BaseAddress`**, **Entra** (mobile client id, tenant, scope).
5. **Build/run** `RaphCare.Mobile` on a device or emulator (**DEBUG** so **`appsettings.Development.json`** applies).
6. **Sign in**: **Entra** account with **Patient** (or allowed) role, or **Phone** registration and OTP from API logs in Development.

---

## 7. Troubleshooting (common)

| Symptom | Check |
|--------|--------|
| **401 / 403** from API | Entra **Audience** / **scope**; user **app role** assignment; token forwarded from mobile (`IAccessTokenProvider`). |
| **Cannot reach API from emulator** | Use **`10.0.2.2`** and correct port; or run API bound to `0.0.0.0` and use machine LAN IP (advanced). |
| **Most tabs show “under construction”** | Feature flags off — use **DEBUG** or set **`FeatureFlags:*`** User Secrets. |
| **SSL errors** to `https://localhost` | Trust dev cert, use **HTTP** profile for local demo, or HTTP cleartext only in dev builds (platform-specific). |
| **Phone OTP never arrives** | Expected in Development — read **API console** for logged code. |

---

## 8. Security reminder

Do **not** commit production secrets. Use **User Secrets**, **Azure Key Vault**, or your CI secret store. Replace sample **Jwt:Secret** and Entra placeholders before any environment with real patient data.
