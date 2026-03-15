# External Integrations

## Payment Gateways

- **Application:** Interface `IPaymentGatewayService` exists in Application.Common.Interfaces.
- **Infrastructure:** Placeholder implementation PaymentGatewayService (no Stripe, PayPal, or other payment SDK in .csproj).
- **Billing:** Invoices and billing entities exist; no wired payment provider.

## Device SDKs

- **Domain:** Device, DeviceType, DeviceAssignment, and reading types (HeartRateReading, BloodPressureReading, GlucoseReading, etc.) are defined. DeviceDbContext and DevicesController provide CRUD for devices.
- **Infrastructure:** IDeviceIntegrationService has a placeholder implementation (DeviceIntegrationService). No third-party device SDKs or device-cloud packages in the solution. No ingestion pipeline or background worker for device readings. DeviceSeeder is a placeholder.

## Messaging Services

- **Application:** Interfaces `IEmailService` and `ISmsService` exist.
- **Infrastructure:** Placeholder implementations EmailService and SmsService (no Twilio, SendGrid, or similar packages in .csproj).
- **Domain:** EmailLog, SMSLog, Notification, Message (in-app) exist. No external messaging integration wired.

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
- **Placeholder implementations in Infrastructure (no external SDKs):** IPaymentGatewayService, IEmailService, ISmsService, IAIService, IDeviceIntegrationService, ITeleSessionService, ISpeechToTextService (AzureSpeechToTextService).
- **Not present:** Payment gateways, device SDKs, messaging providers, AI SDKs, or Speech SDK as NuGet or wired code.
