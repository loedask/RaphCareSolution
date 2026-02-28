# External Integrations

## Payment Gateways

- **Application:** Interface `IPaymentGatewayService` exists in Application.Common.Interfaces. No implementation found in the repo; no Stripe, PayPal, or other payment SDK packages in any .csproj.
- **Billing:** Invoices and billing entities exist; no wired payment provider.

## Device SDKs

- **Domain:** Device, DeviceType, DeviceAssignment, and reading types (HeartRateReading, BloodPressureReading, GlucoseReading, etc.) are defined. DeviceDbContext and DevicesController provide CRUD for devices.
- **Integration:** No third-party device SDKs or device-cloud packages referenced in the solution. No ingestion pipeline or background worker for device readings in the codebase. DeviceSeeder is a placeholder.

## Messaging Services

- **Application:** Interfaces `IEmailService` and `ISmsService` exist. No implementations in the repo; no Twilio, SendGrid, or similar packages in .csproj.
- **Domain:** EmailLog, SMSLog, Notification, Message (in-app) exist. No external messaging integration documented.

## AI Services

- **Application:** `IAIService` with summary generation; command `GenerateSummary` and handler. **Infrastructure:** AIService implements IAIService; implementation is a placeholder (TODO: integrate with Azure OpenAI / GPT). No Azure.AI.*, OpenAI, or other AI SDK packages in the solution.
- **Persistence:** AIDbContext stores WellnessInsight, RiskScore, DashboardSnapshot. AI-related domain entities exist; no external AI call is implemented.

## Authentication (External)

- **Microsoft Entra ID:** JWT Bearer tokens issued by Entra are validated by the API. Configuration via EntraOptions (Authority, Audience, etc.). EntraTokenValidator, EntraUserProvisioningService, EntraRoleMapper in RaphCare.Identity. Packages: Microsoft.AspNetCore.Authentication.JwtBearer, Microsoft.IdentityModel.Protocols.OpenIdConnect, Microsoft.IdentityModel.Tokens, System.IdentityModel.Tokens.Jwt. This is the only external integration with a concrete implementation (token validation and user provisioning from Entra claims).

## Summary

- **Implemented:** Entra ID for JWT validation and user provisioning.
- **Interfaces only (no implementation):** Payment gateway, email, SMS, notification.
- **Placeholder only:** AI service (no SDK); device ingestion (no SDK or job).
- **Not present:** Payment gateways, device SDKs, messaging providers, or AI SDKs as NuGet or code.
