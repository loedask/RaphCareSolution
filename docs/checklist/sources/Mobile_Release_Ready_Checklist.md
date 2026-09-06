# RaphCare.Mobile: release readiness checklist

**Design tokens, typography, and concept to MAUI route mapping** stay in **[`../../Mobile_Concept_Port.md`](../../Mobile_Concept_Port.md)**. Live look and feel follows the **Premium soft** MAUI identity documented there. Use this doc for **product completeness**, **rollout**, and **quality bar** before calling the patient app "ready."

**Structure, DI, configuration, and feature flags:** **[`../../09_Mobile_App_Guide.md`](../../09_Mobile_App_Guide.md)**.

---

## What “ready” means here

**Ready** = planned patient capabilities are **implemented end-to-end** (real APIs where they exist), verified on **Android and iOS** unless a capability is explicitly scoped to one platform, with acceptable **loading / empty / error** behavior, not only a visual mock. Visual polish follows **Premium soft** (see **[`../../Mobile_Concept_Port.md`](../../Mobile_Concept_Port.md)**).

---

## Per screen or sub-flow

Copy the table for each major screen (e.g. list vs detail vs “add”).

| Criterion | Done (Y/N/N/A) | Notes |
|-----------|----------------|-------|
| Feature complete (no stubs) | | |
| API via **Client** + generated **`IClient`** (NSwag regen after contract changes) | | |
| Auth errors (401/403) → sign-in or clear user message | | |
| Loading, empty, error, retry where appropriate | | |
| No raw exception strings in UI | | |
| Offline / degraded network (per product policy) | | |
| Accessibility spot-check (labels, contrast, key flows) | | |
| Visual polish (Premium soft) | | See **[`../../Mobile_Concept_Port.md`](../../Mobile_Concept_Port.md)**; concept is structure/copy reference only. |
| iOS + Android | | N/A only if documented platform scope. |

---

## Feature flags (prod vs pilot)

Flags are defined in **`RaphCare.Mobile.Kernel`** (`FeatureFlags`, `FeatureFlagOptions`) and configured under **`FeatureFlags`** in **`RaphCare.Mobile/appsettings.json`** (overridden by **`appsettings.Development.json`** or User Secrets in DEBUG; see **[`../../09_Mobile_App_Guide.md`](../../09_Mobile_App_Guide.md)**).

**`appsettings.Development.json`** is set so **every flag is `true`** for DEBUG builds (local parity). **`appsettings.json`** (tracked defaults) keeps most verticals off until you choose prod/pilot rollout.

Fill in your rollout targets:

| Flag | Tracked default (`appsettings.json`) | DEBUG dev (`appsettings.Development.json`) | Prod (on/off) | Pilot (on/off) | Notes |
|------|----------------------------------------|---------------------------------------------|---------------|----------------|-------|
| `SettingsEnabled` | `true` | `true` | | | |
| `RecordsEnabled` | `false` | `true` | | | |
| `AppointmentsEnabled` | `false` | `true` | | | |
| `InsuranceEnabled` | `false` | `true` | | | |
| `CareTelehealthEnabled` | `false` | `true` | | | |
| `DevicesEnabled` | `false` | `true` | | | |
| `BillingEnabled` | `false` | `true` | | | |
| `MentalHealthEnabled` | `false` | `true` | | | |
| `FamilyMembersEnabled` | `false` | `true` | | | |
| `AiAssistantEnabled` | `false` | `true` | | | |
| `NotificationsEnabled` | `false` | `true` | | | |

---

## Screen inventory (parity + completeness)

Use the **route to MAUI** table in **[`../../Mobile_Concept_Port.md`](../../Mobile_Concept_Port.md)** as the spine. Extend that doc’s parity checklist with **list / book / detail / add** rows per vertical as you lock each flow.

---

## Cross-cutting backlog (verify before “ready”)

Use this as a reminder list; close each item or mark **N/A** with a short rationale in your release notes.

- **Telehealth RTC:** Android Agora vs **iOS / other platforms** (`ITelehealthRtcSession` registration in **`MobileServiceCollectionExtensions`**). API token path verified via **`tools/verify-agora-rtc-config.ps1`**. iOS still needs **AgoraRtcKit** (xcframework) wiring; see **[`../../10_Agora_Twilio_Setup.md`](../../10_Agora_Twilio_Setup.md)**.
- **AI assistant:** When **`PatientAssistant`** (`AzureOpenAiEndpoint`, `AzureOpenAiApiKey`, `AzureOpenAiDeployment`) is set in API config, **`AIService`** calls Azure OpenAI chat (`gpt-4.1-mini` by default); otherwise **`PlaceholderReply`** is used. Staging setup: **[`../../16_Azure_OpenAI_Setup.md`](../../16_Azure_OpenAI_Setup.md)**.
- **Push:** When **`FirebasePush:ServiceAccountJsonPath`** points to a valid Firebase service-account JSON file, **`FirebasePatientPushNotificationSender`** sends FCM to tokens from **`PatientPushDevices`**; otherwise **`NoOpPatientPushNotificationSender`** runs.
- **Devices:** Vitals sync to API; **offline queue** . **`IVitalsSyncOutbox`** / **`FileVitalsSyncOutbox`** (JSON under app data, max 50 batches) retries on next Devices visit or after a successful sync.
- **Dark mode:** second resource dictionary vs explicit **not in v1** decision (**[`../../Mobile_Concept_Port.md`](../../Mobile_Concept_Port.md)** `.dark` tokens).
- **Deep links:** optional global **NotFound** / bad-route UX (`AppNavigator`).

---

## Related docs

| Doc | Use for |
|-----|---------|
| **[`../../Mobile_Concept_Port.md`](../../Mobile_Concept_Port.md)** | Premium soft identity, historical concept tokens, route map, screen checklist |
| **[`../../09_Mobile_App_Guide.md`](../../09_Mobile_App_Guide.md)** | Where code lives, config, flags, localization, tests |
| **[`Web_And_Mobile_Smoke_Plan.md`](Web_And_Mobile_Smoke_Plan.md)** | API, Web, and Mobile smoke phases (manual phone path until automation) |
| **`11_Devices_BLE_E580_E585.md`** | BLE devices |
| **`10_Agora_Twilio_Setup.md`** | Telehealth / comms setup |
