# External Integrations

## Payment Gateways

- **Application:** Interface `IPaymentGatewayService` exists in Application.Common.Interfaces.
- **Infrastructure:** Placeholder implementation PaymentGatewayService (no Stripe, PayPal, or other payment SDK in .csproj).
- **Billing:** Invoices and billing entities exist; no wired payment provider.

## Device SDKs

- **Domain:** Device, DeviceType, DeviceAssignment, and reading types (HeartRateReading, BloodPressureReading, GlucoseReading, etc.) are defined. DeviceDbContext and DevicesController provide CRUD for devices (staff/provider API).
- **Mobile (patient BLE):** `RaphCare.Mobile` uses **Plugin.BLE** for E580/E585-class wearables — scan, connect, GATT notify subscription; optional PLX SpO₂; sync via **`api/patient/devices`**. See **`docs/11_Devices_BLE_E580_E585.md`**.
- **Provider dashboard:** **`GET api/clinical/patients/{patientId}/device-readings`** — paged vitals for the current clinic (`IClinicContext`). **`GET api/clinical/patients/{patientId}/device-readings/daily-rollup?fromUtc=&toUtc=`** — daily min/max/avg HR and SpO₂ (UTC day buckets; range ≤ 366 days) for lighter charts.
- **Blazor (later):** When building the staff vitals dashboard, call **`GET api/clinical/patients/{patientId}/device-readings`** for raw series and plot **`RecordedAt`** vs **`HeartRateBpm`** / **`SpO2Percent`** (or use **`daily-rollup`** for trend bands). Mobile is the current UI focus; Web can follow this contract after NSwag refresh.
- **FHIR export:** **`GET api/fhir/observations`** (search by `patientId`) and **`GET api/fhir/observations/{id}`** — minimal **Observation** bundle (LOINC heart rate / SpO₂). Same **RequireProvider** + audit pattern as other FHIR routes. **Interop:** Send **`Authorization: Bearer`** (staff token) and **`Accept: application/fhir+json`** on these GETs; the API returns FHIR JSON when that accept header is present (same as other FHIR export routes).
- **Standalone emergency (Y6-class / 4G):** **`POST api/integrations/standalone-emergency/events`** — JSON webhook (device **`serialNumber`**, **`eventType`**, **`occurredAtUtc`**, optional **`externalEventId`** for idempotency, optional GPS). Header **`X-RaphCare-Emergency-Signature`**: hex **HMAC-SHA256** of the raw UTF-8 body when **`StandaloneEmergency:WebhookSharedSecret`** is set; in **Development**, empty secret or **`AllowUnsignedWebhooksInDevelopment: true`** allows unsigned calls for testing. Persists **`DeviceEmergencyEvents`** and texts **emergency contacts** via **`ISmsService`**. Clinicians: **`GET api/clinical/patients/{patientId}/emergency-events`**. See **`docs/13_Patient_Device_Packages_and_Fleet.md`**.
- **Infrastructure:** IDeviceIntegrationService has a placeholder implementation (DeviceIntegrationService). BLE uses patient JWT + **`api/patient/devices`**; 4G emergency uses the integration route above. DeviceSeeder is a placeholder.

## Messaging Services

- **Application:** Interfaces `IEmailService` and `ISmsService` exist.
- **Infrastructure:** **Twilio** is wired for SMS when `Twilio:AccountSid`, `Twilio:AuthToken`, and `Twilio:FromPhoneE164` are all set (`TwilioSmsService`); otherwise **`SmsService`** is a dev placeholder (logs only). **Email** remains **`EmailService`** (placeholder-style unless extended). See **`docs/10_Agora_Twilio_Setup.md`** for Twilio (and Agora) configuration.
- **Domain:** EmailLog, SMSLog, Notification, Message (in-app) exist.

## AI Services

- **Application:** `IAIService` with summary generation; command `GenerateSummary` and handler. **Infrastructure:** AIService implements IAIService; implementation is a placeholder (TODO: integrate with Azure OpenAI / GPT). No Azure.AI.*, OpenAI, or other AI SDK packages in the solution.
- **Persistence:** AIDbContext stores WellnessInsight, RiskScore, DashboardSnapshot. AI-related domain entities exist; no external AI call is implemented.

## Speech-to-Text (Voice Onboarding)

- **Application:** `ISpeechToTextService` (TranscribeAsync returns TranscriptionResult: FullText, ExtractedFields). Used by CreatePatientFromVoiceCommand for voice onboarding.
- **Infrastructure:** AzureSpeechToTextService implements ISpeechToTextService; placeholder implementation (returns sample text and extracted fields). No Azure Speech SDK or other STT packages wired in the solution.

## Authentication (External and API-Issued)

- **Microsoft Entra ID:** JWT Bearer tokens issued by Entra are validated by the API for staff. Configuration via EntraOptions (Authority, Audience, etc.). EntraTokenValidator, EntraUserProvisioningService, EntraRoleMapper in RaphCare.Identity. Packages: Microsoft.AspNetCore.Authentication.JwtBearer, Microsoft.IdentityModel.Protocols.OpenIdConnect, Microsoft.IdentityModel.Tokens, System.IdentityModel.Tokens.Jwt.
- **API-issued JWT (patients):** After OTP verify, API issues a JWT via ITokenService (TokenService). Configuration via JwtOptions (Issuer, Audience, Secret). Used for patient sessions; signing with symmetric key (HmacSha256). No external IdP for this path.

## Summary

- **Implemented:** Entra ID for JWT validation and user provisioning; API-issued JWT for OTP-verified patients (ITokenService, JwtOptions); OTP generation/validation (IOtpService, OtpCodes in IdentityDbContext).
- **Placeholder or partial implementations:** IPaymentGatewayService, IEmailService, IAIService, IDeviceIntegrationService, ITeleSessionService, ISpeechToTextService (AzureSpeechToTextService). **SMS:** Twilio when configured (`docs/10_Agora_Twilio_Setup.md`). **Telehealth RTC tokens:** Agora (`AgoraRtcTokenService`, `docs/10_Agora_Twilio_Setup.md`).
- **Not fully wired:** Payment gateways, device SDKs, production AI SDKs, production Speech SDK (stubs exist).
