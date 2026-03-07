# RaphCare Dev Companion — Change Log

## Date

2026-02-25

## High-level summary of changes

Documentation was re-analyzed against the current codebase and updated so that the RaphCare Dev Companion knowledge files stay aligned with the solution. No new modules or fundamental architecture changes were introduced; updates reflect existing structure (API App folder, Persistence-based user store, Client generated methods, Swagger/operation names, Infrastructure placeholder services).

## New modules added

- None.

## Modules modified

- **RaphCare.API:** Documented App/ folder layout (App/Middleware, App/Extensions, App/Contracts). Controllers remain at root. Swagger and root redirect to `/swagger` documented as Development-only. CreatePatientResponse and SwaggerExtensions (AddRaphCareSwagger, UseRaphCareSwagger) documented.
- **RaphCare.Persistence:** Documented ApplicationUserStore (implements IApplicationUserStore). Documented migration folder layout (IdentityDb, Clinical, InsuranceDb, BillingDb, AIDb). Seed structure unchanged (DatabaseSeeder orchestrator, context-specific seeders).
- **RaphCare.Infrastructure:** Clarified that IApplicationUserStore is not implemented here (implemented in Persistence). Documented CurrentUserService, DateTimeProvider, and placeholder services (Email, SMS, PaymentGateway, AI, DeviceIntegration, TeleSession).
- **RaphCare.Client:** Documented removal of ApiRoutes. Documented Contracts/Interfaces (IPatientService). Documented generated client method names (GetPatientByIdAsync, GetPatientsPaginatedAsync, CreatePatientAsync, UpdatePatientAsync) and that they come from API controller `Name` (operationId). PatientService documented as calling those methods.

## Database changes

- None. BillingDbContext connection string key documented as BillingConnection (or Default). ApplyMigrationsAsync order and location (App/Extensions) documented.

## Auth changes

- None. IApplicationUserStore implementation location corrected: implemented in RaphCare.Persistence (ApplicationUserStore), not in Infrastructure.

## Workflow updates

- **Patient flow (client):** New subsection in 06_Key_Workflows describing PatientService use of generated client methods (GetPatientByIdAsync, GetPatientsPaginatedAsync, CreatePatientAsync, UpdatePatientAsync) and Response/ApiException handling.
- **Patients API:** Documented operation names (GetPatientById, GetPatientsPaginated, CreatePatient, UpdatePatient) and ProducesResponseType for Swagger/NSwag.

## External integrations added/removed

- **Added (documentation only):** Placeholder implementations in Infrastructure for IPaymentGatewayService, IEmailService, ISmsService, IDeviceIntegrationService, ITeleSessionService (no external SDKs wired).
- **Removed:** None.
