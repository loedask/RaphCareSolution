# Agora and Twilio setup (RaphCare)

This guide is for developers or operators who need to configure **Agora** (real-time video for telehealth) and **Twilio** (SMS) for the RaphCare API and mobile app.

**Where configuration lives:** primarily **`RaphCare.API`** (`appsettings.json`, environment variables, or **User Secrets** for local development). The MAUI app does **not** store Agora secrets; it receives **App Id** and **RTC token** from the API after the patient is authenticated.

---

## 1. Twilio (Programmable SMS)

### 1.1 What Twilio is used for in RaphCare

The API registers a single implementation of **`ISmsService`**. When Twilio is fully configured, **`TwilioSmsService`** is used for **all** outbound SMS:

| Feature | Notes |
|--------|--------|
| **OTP** (patient phone sign-in) | `SendOtpHandler` sends the verification code via SMS. |
| **Telehealth reminders** | `SendTelehealthSessionSmsHandler` sends a short SMS with session timing and channel hint. |
| **Future features** | Any code that injects `ISmsService` uses the same Twilio integration. |

If Twilio is **not** fully configured, the API falls back to **`SmsService`**, which **does not** send real SMS (it logs only; in Development it logs the message body for testing).

### 1.2 Prerequisites

- A [Twilio](https://www.twilio.com/) account.
- A **Twilio phone number** capable of **SMS** in the region(s) you need (or a Messaging Service with a sender).
- For production: compliance with your organization’s policies for SMS, consent, and opt-out where required.

### 1.3 Obtain credentials in the Twilio Console

1. Sign in to the [Twilio Console](https://console.twilio.com/).
2. On the dashboard, copy **Account SID** and **Auth Token** (treat the Auth Token as a secret; rotate if exposed).
3. **Phone number:** Navigate to **Phone Numbers → Manage → Active numbers** (or purchase a number). Copy the number in **E.164** format (e.g. `+15551234567`). This value maps to **`FromPhoneE164`** in configuration.

Optional: You can use a **Messaging Service** with a sender pool; the current RaphCare code sends **from** a single E.164 `From` number via the Twilio REST API (`MessageResource.CreateAsync`). Ensure the number you put in config is SMS-capable and allowed to send to your test users’ countries.

### 1.4 Configuration keys (API)

Bind the **`Twilio`** section (see `TwilioSmsOptions.SectionName` in Application). All three values must be non-empty for Twilio to be **enabled**:

| Key | Example | Description |
|-----|---------|-------------|
| `Twilio:AccountSid` | `ACxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx` | Account SID from the console. |
| `Twilio:AuthToken` | `(secret)` | Auth token from the console. |
| `Twilio:FromPhoneE164` | `+15551234567` | Sender number in E.164. |

**Example (`appsettings.Development.json` or User Secrets — do not commit real secrets):**

```json
{
  "Twilio": {
    "AccountSid": "ACxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "AuthToken": "your_auth_token",
    "FromPhoneE164": "+15551234567"
  }
}
```

**Environment variables** (e.g. Azure App Service, Docker): use the same hierarchy with `__` as the separator:

- `Twilio__AccountSid`
- `Twilio__AuthToken`
- `Twilio__FromPhoneE164`

### 1.5 How the API chooses Twilio vs placeholder

In **`RaphCare.Infrastructure.DependencyInjection`**, the bound **`TwilioSmsOptions`** is evaluated with **`IsEnabled`**, which requires **AccountSid**, **AuthToken**, and **FromPhoneE164**. If `IsEnabled` is true, **`TwilioSmsService`** is registered; otherwise **`SmsService`** (placeholder).

Implementation reference: `RaphCare.Infrastructure.Services.TwilioSmsService` (Twilio .NET SDK).

### 1.6 Local development tips

- Use **`dotnet user-secrets`** on **`RaphCare.API`** (or a local `appsettings.Development.json` that is **gitignored** if your team allows it) for Twilio secrets.
- **OTP testing:** With Twilio enabled, each OTP request sends a real SMS (billable). For cheap local iteration without SMS, leave Twilio incomplete to use the placeholder and read OTP from API logs in Development (see `SmsService`).

### 1.7 Telehealth SMS validation

The **send telehealth SMS** command checks **`TwilioSmsOptions.IsEnabled`** before sending. If SMS is not configured, the API returns a validation error indicating that **`Twilio:AccountSid`**, **`Twilio:AuthToken`**, and **`Twilio:FromPhoneE164`** must be set (see `SendTelehealthSessionSmsHandler`).

### 1.8 Troubleshooting (Twilio)

| Symptom | Things to check |
|--------|------------------|
| SMS never arrives | Wrong `FromPhoneE164`; number not SMS-enabled; destination country blocked; Twilio trial only sending to verified numbers. |
| API still uses placeholder | One of the three Twilio keys is missing or empty; restart the API after changing config. |
| 401 / auth errors from Twilio | Incorrect **Auth Token** or **Account SID**. |
| OTP works but telehealth SMS fails | Same `ISmsService`; verify patient profile has a **phone number** and Twilio is enabled. |

---

## 2. Agora (RTC / video for telehealth)

### 2.1 What Agora is used for in RaphCare

- **API:** Builds **RTC tokens** (using your **App ID** and **App Certificate**) so clients can join a **channel** securely. Token generation is in **`AgoraRtcTokenService`** (`RaphCare.Infrastructure`), using the **`AgoraDynamicKey`** / AgoraIO token utilities.
- **Mobile:** **Android** loads the native **`io.agora.rtc:full-sdk`** (Maven) and joins the channel via **`AgoraAndroidTelehealthRtcSession`** (no Agora UI Kit). Other platforms may use a **no-op** RTC implementation until native SDK work is added.

**Security rule:** The **App Certificate** must **only** exist on the server. Never ship it in mobile apps or public repos.

### 2.2 Prerequisites

- An [Agora](https://www.agora.io/) account and Console access.
- A project with **RTC** (Real-Time Communication) enabled.

### 2.3 Obtain App ID and App Certificate

1. Sign in to [Agora Console](https://console.agora.io/).
2. Create a project (or open an existing one).
3. Copy the **App ID** (public identifier; safe to expose to clients in join info).
4. Enable the **primary certificate** (or secondary, per your security process) and copy the **App Certificate**. Store it as a **secret** in server configuration only.

If you rotate the certificate in Agora, update the API configuration and redeploy; existing tokens will stop working after expiry.

### 2.4 Configuration keys (API)

Bind the **`AgoraRtc`** section (`AgoraRtcOptions.SectionName` in Infrastructure):

| Key | Description |
|-----|-------------|
| `AgoraRtc:AppId` | Agora project App ID (string). |
| `AgoraRtc:AppCertificate` | Primary (or chosen) App Certificate; **server-side only**. |

**Example:**

```json
{
  "AgoraRtc": {
    "AppId": "your_agora_app_id",
    "AppCertificate": "your_app_certificate"
  }
}
```

**Environment variables:**

- `AgoraRtc__AppId`
- `AgoraRtc__AppCertificate`

### 2.5 Token generation and join flow (behavior)

- **`ITelehealthRtcTokenGenerator.IsConfigured`** is true only when both **AppId** and **AppCertificate** are set.
- **`GetTelehealthJoinInfoHandler`** builds:
  - **Channel name** from `TeleSession.SessionExternalId`, or `raph-tele-{session.Id:N}` if empty.
  - **Uid** from the query’s optional `Uid`, or a **stable uint** derived from the patient id.
  - **RtcToken** via **`BuildRtcToken(channel, uid, ttlSeconds)`** (default TTL **3600** seconds), **publisher** role.
- The DTO includes **`RtcConfigured`**, **`AppId`**, **`RtcToken`**, **`TokenExpiresAtUnix`**, etc., so the client can join with the native SDK.

If Agora is **not** configured on the server, join info may still return **AppId** as null and no token; the mobile app should treat that as “RTC not ready” (see DTO and VM handling).

### 2.6 Mobile app (Android)

- **SDK:** `RaphCare.Mobile.csproj` references **`io.agora.rtc:full-sdk`** version **4.5.2** via **`AndroidMavenLibrary`** (`Bind="false"` — JNI/reflection-based integration).
- **Registration:** `MobileServiceCollectionExtensions` registers **`AgoraAndroidTelehealthRtcSession`** on Android.
- **Permissions:** Camera (and related) must be granted at runtime; **AndroidManifest** includes camera permission; **iOS/MacCatalyst Info.plist** includes camera usage strings for builds that include those targets.

**Note:** Native Agora video is **implemented for Android** in this solution; **iOS / Mac Catalyst / Windows** may still show placeholders or no-op RTC until equivalent native wiring is added.

### 2.7 Testing Agora end-to-end

1. Configure **AgoraRtc** on the API and deploy or run locally with secrets.
2. Ensure a **telehealth session** exists for the patient and **join info** returns a non-empty **RtcToken** and **`RtcConfigured: true`**.
3. Run the **Android** app, open **Care & telehealth**, start video for that session, and verify local preview and (with a second participant or Agora’s sample) remote video.

### 2.8 Troubleshooting (Agora)

| Symptom | Things to check |
|--------|------------------|
| `InvalidOperationException` about Agora not configured | **AppId** or **AppCertificate** missing in API config. |
| Join fails with error 110 / token | Token expired; wrong App ID or certificate; channel or uid mismatch; clock skew on server. |
| No video on Android | Camera permission; physical device vs emulator with camera; Logcat **`RaphCareRtc`** logs from `AgoraAndroidTelehealthRtcSession`. |
| Certificate leaked | Rotate in Agora Console; revoke old secrets in source control history if committed. |

---

## 3. Quick checklist (new environment)

**Twilio**

- [ ] Account SID, Auth Token, and SMS-capable **From** number in E.164.
- [ ] Values set under **`Twilio`** for the API process (env or appsettings + secrets).
- [ ] Restart API; verify a test OTP or telehealth SMS if appropriate.

**Agora**

- [ ] RTC project created; **App ID** and **App Certificate** copied.
- [ ] **`AgoraRtc`** section set on the API; certificate **never** in mobile repo.
- [ ] API returns join info with **`RtcConfigured`** and token for a test session.
- [ ] Android app tested on device/emulator with camera.

---

## 4. Related code and docs

| Area | Location |
|------|----------|
| Twilio options | `RaphCare.Application/Common/Configuration/TwilioSmsOptions.cs` |
| Twilio SMS | `RaphCare.Infrastructure/Services/TwilioSmsService.cs` |
| SMS registration | `RaphCare.Infrastructure/DependencyInjection.cs` |
| Agora options | `RaphCare.Infrastructure/Configuration/AgoraRtcOptions.cs` |
| Agora tokens | `RaphCare.Infrastructure/Telehealth/AgoraRtcTokenService.cs` |
| Join info query | `RaphCare.Application/Features/PatientTelehealth/Queries/GetTelehealthJoinInfo/` |
| Telehealth SMS command | `RaphCare.Application/Features/PatientTelehealth/Commands/SendTelehealthSessionSms/` |
| API sample config | `RaphCare.API/appsettings.json` |
| Android Agora | `RaphCare.Mobile/Platforms/Android/Telehealth/AgoraAndroidTelehealthRtcSession.cs` |

For broader integration inventory (may be partially superseded by Twilio and Agora wiring), see **`docs/08_External_Integrations.md`**.
