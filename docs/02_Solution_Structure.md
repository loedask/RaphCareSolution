# Solution Structure

## Full Project List

| Project | Purpose |
|--------|---------|
| RaphCare.API | ASP.NET Core REST API; controllers, middleware, Swagger |
| RaphCare.Application | Use cases (MediatR commands/queries), DTOs, validators, behaviors, interfaces |
| RaphCare.Domain | Entities, value objects, domain events; no dependencies |
| RaphCare.Infrastructure | Implementations of application interfaces (e.g. AIService, persistence interceptors) |
| RaphCare.Persistence | DbContexts, EF configurations, repositories, migrations, seeders |
| RaphCare.Identity | Entra ID JWT validation, user provisioning, role mapping |
| RaphCare.Client | Shared API client (generated client, base HTTP service, feature services, AutoMapper) |
| RaphCare.Web | Blazor WebAssembly front-end |
| RaphCare.Mobile | .NET MAUI mobile app |

## Folder Breakdown by Project

### RaphCare.API
- **Controllers/** — REST controllers (Patients, Appointments, Clinical, Devices, Insurance, Billing, Telemedicine, MentalHealth, AI, Reporting, Auth, VoiceOnboarding); all use `api/[controller]` and MediatR; AuthController at `api/auth/otp` (POST send, POST verify); VoiceOnboardingController at `api/onboarding` (POST voice, multipart); PatientsController uses `Name` on actions for Swagger operationId (GetPatientById, GetPatientsPaginated, CreatePatient, UpdatePatient)
- **App/Middleware/** — ExceptionHandlingMiddleware, TenantResolutionMiddleware, AuditMiddleware
- **App/Extensions/** — DatabaseMigrationExtensions (ApplyMigrationsAsync for Development), SwaggerExtensions (AddRaphCareSwagger, UseRaphCareSwagger; root redirect to /swagger in Development)
- **App/Contracts/** — CreatePatientResponse (OpenAPI response type for POST patients)

### RaphCare.Application
- **Common/** — Interfaces (IRepository, IUnitOfWork, ICurrentUserService, IAIService, IApplicationUserStore, IUserProvisioningService, IDomainEventDispatcher, IDateTimeProvider, IPaymentGatewayService, IEmailService, ISmsService, INotificationService, IOtpService, ITokenService, ISpeechToTextService), DTOs (PagedResult, BaseDto), Behaviors (Logging, Performance, Authorization, Validation, Transaction), Exceptions
- **Features/** — Vertical slices per feature (e.g. Patients, Appointments, Clinical, Devices, Insurance, Billing, Telemedicine, Communication, MentalHealth, AI, Reporting, Auth, Onboarding); each contains Commands, Queries, DTOs, Validators, Handlers

### RaphCare.Domain
- **Common/** — BaseEntity, AggregateRoot, ValueObject, Enumeration, DomainEvent, interfaces
- **Identity/** — ApplicationUser, Role, Permission, UserRole, RolePermission, RefreshTokenRecord, UserSession, AuditLog, LoginAudit, AccessPolicy, OtpCode
- **Organization/** — Clinic, Department, Facility, Provider, Administrator, Therapist, ServiceOffering, ProviderSchedule, AvailabilityBlock, SupportStaff
- **Patients/** — Patient, PatientProfile, InsuranceProfile, Address, EmergencyContact, Allergy, Medication, MedicalHistory, VoiceRecording, etc.
- **Clinical/** — Appointment, Visit, CarePlan, Prescription, ClinicalNote, LabResult, Diagnosis, Procedure, etc.
- **Communication/** — Message, Conversation, Notification, EmailLog, SMSLog, etc.
- **Devices/** — Device, DeviceType, DeviceAssignment, DeviceReading, HeartRateReading, BloodPressureReading, etc.
- **Insurance/** — InsurancePlan, InsuranceProfile (domain), Subscription, Claim, CoverageRule, etc.
- **Billing/** — Invoice, PaymentTransaction, BillingProfile, etc.
- **Telemedicine/** — TeleSession, TeleSessionParticipant, PreVisitQuestionnaire, etc.
- **MentalHealth/** — MentalHealthAssessment, TherapySession, MoodLog, etc.
- **AI/** — WellnessInsight, RiskScore, TrendAnalysis, etc. (domain entities for AI context)
- **Reporting/** — DashboardSnapshot, ClinicPerformance, etc.
- **Compliance/** — AuditTrail, ConsentGrant, DataRetentionPolicy, etc.
- **InfrastructureEntities/** — BackgroundJob, FeatureFlag, HealthCheckLog, etc. (support entities)

### RaphCare.Infrastructure
- **Persistence/Interceptors/** — AuditableEntityInterceptor, SoftDeleteInterceptor, DomainEventDispatcherInterceptor
- **Persistence/Configurations/** — EF configurations for entities (in Infrastructure; e.g. OtpCodeConfiguration, VoiceRecordingConfiguration)
- **Services/** — Implementations (e.g. AIService, OtpService, TokenService, AzureSpeechToTextService placeholder)
- **Identity/** — (IApplicationUserStore is implemented in Persistence.ApplicationUserStore)

### RaphCare.Persistence
- **DbContexts:** IdentityDbContext (Users, Roles, Permissions, UserRoles, RolePermissions, OtpCodes), ClinicalDbContext (Patients, Clinics, Appointments, Visits, CarePlans, TeleSessions, Messages, VoiceRecordings), DeviceDbContext, InsuranceDbContext, BillingDbContext, AIDbContext
- **ApplicationUserStore** — Implements IApplicationUserStore against IdentityDbContext (FindByEntraObjectIdAsync, CreateAsync, UpdateAsync)
- **Repositories/** — EfRepository<TEntity, TContext>; repository registration per entity/context in DependencyInjection
- **Seed/** — DatabaseSeeder (orchestrator), IdentitySeeder, ClinicalSeeder, InsuranceSeeder, DeviceSeeder (placeholder), BillingSeeder (placeholder)
- **Migrations/** — EF Core migrations (per DbContext); under Migrations/ and per-context subfolders (IdentityDb, Clinical, InsuranceDb, BillingDb, AIDb)

### RaphCare.Identity
- **Entra/** — EntraOptions, EntraTokenValidator, EntraUserProvisioningService, EntraRoleMapper
- **DependencyInjection.cs** — Registers Entra options, JWT Bearer validation, user provisioning, role mapper

### RaphCare.Client
- **Contracts/** — Response&lt;T&gt;, ApiException, IAccessTokenProvider (provides access token for Bearer auth when host implements it)
- **Contracts/Interfaces/** — IPatientService (service contracts)
- **Services/Base/Generated/** — NSwag-generated Client and IClient (ClientService.cs), generated DTOs (PatientDto, PatientDtoPagedResult, CreatePatientResponse); method names follow API operation names (GetPatientByIdAsync, GetPatientsPaginatedAsync, CreatePatientAsync, UpdatePatientAsync)
- **Services/Base/** — Partial IClient/Client (expose HttpClient); BaseHttpService (wraps IClient and HttpClient; generic Get/Post/Put/Delete returning Response&lt;T&gt;); BearerTokenHandler (DelegatingHandler that attaches Bearer token from IAccessTokenProvider when useBearerToken is true)
- **Services/** — PatientService (example); implements IPatientService; calls generated client methods (GetPatientByIdAsync, GetPatientsPaginatedAsync, CreatePatientAsync, UpdatePatientAsync); inject IClient and IMapper
- **Models/** — ViewModels and request DTOs (e.g. PatientViewModel, CreatePatientRequest)
- **Mappings/** — AutoMapper profiles (DTOs to ViewModels)
- **ServiceRegistration.cs** — AddRaphCareClient(configureHttpClient?, useBearerToken); when useBearerToken is true, adds BearerTokenHandler and requires IAccessTokenProvider to be registered in the host (e.g. Mobile)

### RaphCare.Web
- Blazor WASM structure (App, components, pages, etc.); references RaphCare.Client

### RaphCare.Mobile.Kernel
- **net10.0** class library (no MAUI): `AuthResult`, `FeatureFlags`, `FeatureFlagOptions` — same CLR namespaces as before (`RaphCare.Mobile.Core.Shared.*`) so the MAUI app references this assembly for shared, testable primitives. Unit tests target Kernel only.

### RaphCare.Mobile.Tests
- **xUnit** project targeting **net10.0**; references **RaphCare.Mobile.Kernel** (not the MAUI app) so `dotnet test` does not run MAUI Resizetizer.

### RaphCare.Mobile
- **Core/Features/** — Feature-specific Views and ViewModels (Auth, Home, Hybrid/BlazorHostPage, Records, Appointments, Insurance, Settings). Namespaces: `RaphCare.Mobile.Core.Features.*.Views` / `.ViewModels`. Auth services: EntraAuthService, IAuthService, EntraAuthOptions, SecureStorageAccessTokenProvider under **Core/Shared/Services/Auth**.
- **Core/Shared/** — AppNavigator, Services/Auth, Views/UnderConstructionPage, **ViewModels** (`BaseViewModel`), **Controls** (MAUI XAML; `RaphCare.Mobile.Core.Shared.Controls`).
- **Core/Infrastructure/** — `MobileServiceCollectionExtensions.AddRaphCareMobile`, `MobileServiceHub` (DI resolution for Shell pages).
- **Blazor/** — Razor UI for BlazorWebView (`Routes.razor`, `Layout/`, `Pages/`; `RaphCare.Mobile.Blazor`).
- **Resources/Strings/** — `AppResources.resx` + `AppResources.cs` for localization (`RaphCare.Mobile.Resources.Strings.AppResources`).
- **Core/Converters/** — InvertedBoolConverter, StringNotEmptyConverter (`RaphCare.Mobile.Core.Converters`; merged in App.xaml).
- **AppShell** — Shell; `AppNavigator.RegisterAllRoutes()` registers auth, main, hybrid (`BlazorHostPage`), and shared routes.
- **MauiProgram** — Configuration: `appsettings.json`, optional `appsettings.Development.json` (DEBUG), User Secrets; `AddRaphCareMobile`; `FeatureFlags.Initialize` after build; `AddRaphCareClient(..., useBearerToken: true)`; `AddMauiBlazorWebView`.
- References **RaphCare.Mobile.Kernel**, **RaphCare.Client**; bearer-token auth via `IAccessTokenProvider`.

See **docs/09_Mobile_App_Guide.md** for configuration, secrets, flags, and testing. For **Agora** (telehealth RTC) and **Twilio** (SMS) setup on the API, see **docs/10_Agora_Twilio_Setup.md**. For **BLE wearables** (E580/E585-class) on the mobile app, see **docs/11_Devices_BLE_E580_E585.md**.

## Responsibilities Summary

- **Domain:** Entity and value-object definitions only; no infrastructure
- **Application:** Orchestration via MediatR; defines interfaces consumed by Infrastructure/Persistence/Identity
- **Infrastructure:** Cross-cutting implementations (interceptors, external service adapters, user store)
- **Persistence:** Data access (DbContexts, repositories, migrations, seeding)
- **Identity:** Auth (Entra JWT validation, user provisioning, role mapping)
- **API:** HTTP surface, middleware, auth policies
- **Client:** Reusable API client for Web and Mobile
