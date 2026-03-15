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

### RaphCare.Mobile
- **Core/Features/** — Feature-specific Views, ViewModels, Services, Models: Auth (Landing, RegisterOptions, RegisterEmail, VerifyEmail, SignIn; EntraAuthService, EntraAuthOptions, IAuthService, SecureStorageAccessTokenProvider), Home, Records, Appointments, Insurance, Settings. View namespaces: RaphCare.Mobile.Features.*.Views for Shell/routing; ViewModels/Services use RaphCare.Mobile.Core.Features.*.
- **Core/Shared/** — AppNavigator (Navigation; RegisterAllRoutes, GoToFeatureAsync for feature-flag aware navigation), Services/Auth (EntraAuthOptions, IAuthService, EntraAuthService used by MauiProgram), Services/FeatureFlags (FeatureFlags), Views/UnderConstructionPage (RaphCare.Mobile.Shared.Views), Components (GradientButton, CardView, InputField, IconButton).
- **Core/Converters/** — InvertedBoolConverter, StringNotEmptyConverter (namespace RaphCare.Mobile.Core.Converters; referenced in App.xaml).
- **Core/ViewModels/** — BaseViewModel.
- **AppShell** — Shell with FlyoutBehavior Disabled; routes LandingPage (auth) and HomePage (main); AppNavigator.RegisterAllRoutes() for all feature routes. References RaphCare.Mobile.Features.Auth.Views, RaphCare.Mobile.Features.Home.Views.
- **MauiProgram** — Registers Entra auth (Core.Shared.Services.Auth), SecureStorageAccessTokenProvider as IAccessTokenProvider, view models and pages; AddRaphCareClient(..., useBearerToken: true). MAUI Blazor Hybrid (AddMauiBlazorWebView).
- References RaphCare.Client; implements bearer-token auth via IAccessTokenProvider.

## Responsibilities Summary

- **Domain:** Entity and value-object definitions only; no infrastructure
- **Application:** Orchestration via MediatR; defines interfaces consumed by Infrastructure/Persistence/Identity
- **Infrastructure:** Cross-cutting implementations (interceptors, external service adapters, user store)
- **Persistence:** Data access (DbContexts, repositories, migrations, seeding)
- **Identity:** Auth (Entra JWT validation, user provisioning, role mapping)
- **API:** HTTP surface, middleware, auth policies
- **Client:** Reusable API client for Web and Mobile
