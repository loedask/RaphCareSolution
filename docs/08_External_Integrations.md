# External Integrations

## Payment Gateways

- **Application:** Interface `IPaymentGatewayService` supports placeholder charge plus hosted checkout initialize/verify (`PaymentCheckoutSession`, `PaymentChargeVerification`).
- **Infrastructure:** **Paystack** when `Paystack:SecretKey` is set (`PaystackPaymentGatewayService` via typed HttpClient). Otherwise **`PaymentGatewayService`** is a log-only placeholder.
- **Patient care plans:** `POST api/patient/billing/checkout/initialize` starts Paystack hosted checkout for Essential/Complete. `POST api/patient/billing/checkout/confirm` verifies the reference and activates the plan. `POST api/webhooks/paystack` handles `charge.success`. Free upgrades still use `POST api/patient/billing/upgrade` without payment. When Paystack is configured, paid `upgrade` is rejected so callers must use checkout.
- **Config:** `Paystack:SecretKey`, optional `Paystack:PublicKey`, optional `Paystack:CallbackUrl`. Use test keys on staging.
- **Billing catalog:** Invoices and billing entities exist; site software invoicing via Paystack is a follow-up on top of the Ops price catalog.

## Device SDKs

- **Domain:** Device, DeviceType, DeviceAssignment, and reading types (HeartRateReading, BloodPressureReading, GlucoseReading, etc.) are defined. DeviceDbContext and DevicesController provide CRUD for devices (staff/provider API).
- **Mobile (patient BLE):** `RaphCare.Mobile` uses **Plugin.BLE** for E580/E585-class wearables — scan, connect, GATT notify subscription; optional PLX SpO₂; sync via **`api/patient/devices`**. See **`docs/11_Devices_BLE_E580_E585.md`**. Capability targets (full band, live vitals, background): **`docs/14_Wearable_Capability_Catalog.md`**. HBand vendor SDK path: **`docs/12_HBand_SDK_Integration.md`**.
- **Provider dashboard:** **`GET api/clinical/patients/{patientId}/device-readings`** — paged vitals for the current clinic (`IClinicContext`). **`GET api/clinical/patients/{patientId}/device-readings/daily-rollup?fromUtc=&toUtc=`** — daily min/max/avg HR and SpO₂ (UTC day buckets; range ≤ 366 days) for lighter charts.
- **Blazor (later):** When building the staff vitals dashboard, call **`GET api/clinical/patients/{patientId}/device-readings`** for raw series and plot **`RecordedAt`** vs **`HeartRateBpm`** / **`SpO2Percent`** (or use **`daily-rollup`** for trend bands). Mobile is the current UI focus; Web can follow this contract after NSwag refresh.
- **FHIR export:** **`GET api/fhir/observations`** (search by `patientId`) and **`GET api/fhir/observations/{id}`** — minimal **Observation** bundle (LOINC heart rate / SpO₂). Same **RequireProvider** + audit pattern as other FHIR routes. **Interop:** Send **`Authorization: Bearer`** (staff token) and **`Accept: application/fhir+json`** on these GETs; the API returns FHIR JSON when that accept header is present (same as other FHIR export routes).
- **Standalone emergency (Y6-class / 4G):** **`POST api/integrations/standalone-emergency/events`** — JSON webhook (device **`serialNumber`**, **`eventType`**, **`occurredAtUtc`**, optional **`externalEventId`** for idempotency, optional GPS). Header **`X-RaphCare-Emergency-Signature`**: hex **HMAC-SHA256** of the raw UTF-8 body when **`StandaloneEmergency:WebhookSharedSecret`** is set; in **Development**, empty secret or **`AllowUnsignedWebhooksInDevelopment: true`** allows unsigned calls for testing. Persists **`DeviceEmergencyEvents`** and texts **emergency contacts** via **`ISmsService`**. Clinicians: **`GET api/clinical/patients/{patientId}/emergency-events`** and clinic board **`GET api/clinical/emergency-events`**. See **`docs/13_Patient_Device_Packages_and_Fleet.md`**.
- **Infrastructure:** IDeviceIntegrationService has a placeholder implementation (DeviceIntegrationService). BLE uses patient JWT + **`api/patient/devices`**; 4G emergency uses the integration route above. DeviceSeeder is a placeholder.

## Messaging Services

- **Application:** Interfaces `IEmailService` and `ISmsService` exist.
- **Infrastructure:** **Twilio** is wired for SMS when `Twilio:AccountSid`, `Twilio:AuthToken`, and `Twilio:FromPhoneE164` are all set (`TwilioSmsService`); otherwise **`SmsService`** is a dev placeholder (logs only). **Email** remains **`EmailService`** (placeholder-style unless extended). See **`docs/10_Agora_Twilio_Setup.md`** for Twilio (and Agora) configuration.
- **Domain:** EmailLog, SMSLog, Notification, Message (in-app) exist.

## AI Services

- **Application:** `IAIService` with summary generation, discharge drafts, and patient assistant chat. **Infrastructure:** `AIService` calls Azure OpenAI chat completions (`gpt-4.1-mini` Global Standard) when `PatientAssistant` endpoint, key, and deployment are set; otherwise placeholders. No Azure.AI.* or OpenAI SDK packages. See **docs/16_Azure_OpenAI_Setup.md**.
- **Persistence:** AIDbContext stores WellnessInsight, RiskScore, DashboardSnapshot. Those stores are separate from the chat call.

## Speech-to-Text (Voice Onboarding)

- **Application:** `ISpeechToTextService` (TranscribeAsync returns TranscriptionResult: FullText, ExtractedFields). Used by CreatePatientFromVoiceCommand for voice onboarding.
- **Infrastructure:** AzureSpeechToTextService implements ISpeechToTextService; placeholder implementation (returns sample text and extracted fields). No Azure Speech SDK or other STT packages wired in the solution.

## File storage (photos and voice)

- **Application:** `IObjectStorage`, `IPatientProfilePhotoStorage`, `IVoiceRecordingStorage`.
- **Infrastructure:** Azure Blob when `AzureStorage:ConnectionString` is set (`AzureBlobObjectStorage`). Development without that setting uses `App_Data/object-store` (`LocalFileObjectStorage`). Staging and Production require the connection string.
- **API:** patient photo at `api/patient/profile/photo`. Staff photo at `GET api/admin/clinics/{id}/patients/{patientId}/photo` after clinic membership and patient-in-clinic checks. Voice onboarding stores a private key on `VoiceRecording.StorageUrl`.
- **Setup:** `docs/17_Azure_Blob_Storage.md` and `scripts/New-RaphCareAzureBlobStorage.ps1`.

## Authentication (External and API-Issued)

- **Microsoft Entra ID:** JWT Bearer tokens issued by Entra are validated by the API for staff. Configuration via EntraOptions (Authority, Audience, etc.). EntraTokenValidator, EntraUserProvisioningService, EntraRoleMapper in RaphCare.Identity. Packages: Microsoft.AspNetCore.Authentication.JwtBearer, Microsoft.IdentityModel.Protocols.OpenIdConnect, Microsoft.IdentityModel.Tokens, System.IdentityModel.Tokens.Jwt.
- **API-issued JWT (patients):** After OTP verify, API issues a JWT via ITokenService (TokenService). Configuration via JwtOptions (Issuer, Audience, Secret). Used for patient sessions; signing with symmetric key (HmacSha256). No external IdP for this path.

## Summary

- **Implemented:** Entra ID for JWT validation and user provisioning; API-issued JWT for OTP-verified patients (ITokenService, JwtOptions); OTP generation/validation (IOtpService, OtpCodes in IdentityDbContext).
- **Placeholder or partial implementations:** IEmailService, IAIService, IDeviceIntegrationService, ITeleSessionService, ISpeechToTextService (AzureSpeechToTextService). **SMS:** Twilio when configured (`docs/10_Agora_Twilio_Setup.md`). **Telehealth RTC tokens:** Agora (`AgoraRtcTokenService`, `docs/10_Agora_Twilio_Setup.md`). **Payments:** Paystack when `Paystack:SecretKey` is set; otherwise payment gateway placeholder.
- **Not fully wired:** Site software subscription invoices via Paystack, device SDKs, production AI SDKs, production Speech SDK (stubs exist).
