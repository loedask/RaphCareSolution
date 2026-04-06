# RaphCare.Mobile — release readiness checklist

**Design tokens, typography, and concept ↔ MAUI route mapping** stay in **`Mobile_Concept_Port.md`**. Use this doc for **product completeness**, **rollout**, and **quality bar** before calling the patient app “ready.”

**Structure, DI, configuration, and feature flags:** **`09_Mobile_App_Guide.md`**.

---

## What “ready” means here

**Ready** = planned patient capabilities are **implemented end-to-end** (real APIs where they exist), verified on **Android and iOS** unless a capability is explicitly scoped to one platform, with acceptable **loading / empty / error** behavior—not only visual parity with the React concept.

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
| Visual parity vs concept | | See **`Mobile_Concept_Port.md`**; note intentional deltas. |
| iOS + Android | | N/A only if documented platform scope. |

---

## Feature flags (prod vs pilot)

Flags are defined in **`RaphCare.Mobile.Kernel`** (`FeatureFlags`, `FeatureFlagOptions`) and configured under **`FeatureFlags`** in **`RaphCare.Mobile/appsettings.json`** (overridden by **`appsettings.Development.json`** or User Secrets in DEBUG—see **`09_Mobile_App_Guide.md`**).

Fill in your rollout targets:

| Flag | Prod (on/off) | Pilot (on/off) | Notes |
|------|---------------|----------------|-------|
| `SettingsEnabled` | | | |
| `RecordsEnabled` | | | |
| `AppointmentsEnabled` | | | |
| `InsuranceEnabled` | | | |
| `CareTelehealthEnabled` | | | |
| `DevicesEnabled` | | | |
| `BillingEnabled` | | | |
| `MentalHealthEnabled` | | | |
| `FamilyMembersEnabled` | | | |
| `AiAssistantEnabled` | | | |
| `NotificationsEnabled` | | | |

---

## Screen inventory (parity + completeness)

Use the **route ↔ MAUI** table in **`Mobile_Concept_Port.md`** as the spine. Extend that doc’s parity checklist with **list / book / detail / add** rows per vertical as you lock each flow.

---

## Cross-cutting backlog (verify before “ready”)

Use this as a reminder list; close each item or mark **N/A** with a short rationale in your release notes.

- **Telehealth RTC:** Android Agora vs **iOS / other platforms** (`ITelehealthRtcSession` registration in **`MobileServiceCollectionExtensions`**).
- **AI assistant:** backend **`IAIService`** / real provider vs placeholder replies.
- **Push:** **`IPatientPushNotificationSender`** implementation vs no-op.
- **Devices:** vitals pipeline to API; **offline queue / retry / idempotency** if required for field use.
- **Dark mode:** second resource dictionary vs explicit **not in v1** decision (**`Mobile_Concept_Port.md`** `.dark` tokens).
- **Deep links:** optional global **NotFound** / bad-route UX (`AppNavigator`).

---

## Related docs

| Doc | Use for |
|-----|---------|
| **`Mobile_Concept_Port.md`** | Colors, type, radii, shadows/gradients, route map, visual parity checklist |
| **`09_Mobile_App_Guide.md`** | Where code lives, config, flags, localization, tests |
| **`11_Devices_BLE_E580_E585.md`** | BLE devices |
| **`10_Agora_Twilio_Setup.md`** | Telehealth / comms setup |
