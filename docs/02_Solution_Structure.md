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
- **Controllers/** — REST controllers (Patients, Appointments, Clinical, Devices, Insurance, Billing, Telemedicine, MentalHealth, AI, Reporting); all use `api/[controller]` and MediatR
- **Middleware/** — ExceptionHandlingMiddleware, TenantResolutionMiddleware, AuditMiddleware
- **Extensions/** — DatabaseMigrationExtensions (ApplyMigrationsAsync for Development)

### RaphCare.Application
- **Common/** — Interfaces (IRepository, IUnitOfWork, ICurrentUserService, IAIService, IApplicationUserStore, IUserProvisioningService, IDomainEventDispatcher, IDateTimeProvider, IPaymentGatewayService, IEmailService, ISmsService, INotificationService), DTOs (PagedResult, BaseDto), Behaviors (Logging, Performance, Authorization, Validation, Transaction), Exceptions
- **Features/** — Vertical slices per feature (e.g. Patients, Appointments, Clinical, Devices, Insurance, Billing, Telemedicine, Communication, MentalHealth, AI, Reporting); each contains Commands, Queries, DTOs, Validators, Handlers

### RaphCare.Domain
- **Common/** — BaseEntity, AggregateRoot, ValueObject, Enumeration, DomainEvent, interfaces
- **Identity/** — ApplicationUser, Role, Permission, UserRole, RolePermission, RefreshTokenRecord, UserSession, AuditLog, LoginAudit, AccessPolicy
- **Organization/** — Clinic, Department, Facility, Provider, Administrator, Therapist, ServiceOffering, ProviderSchedule, AvailabilityBlock, SupportStaff
- **Patients/** — Patient, PatientProfile, InsuranceProfile, Address, EmergencyContact, Allergy, Medication, MedicalHistory, etc.
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
- **Persistence/Configurations/** — EF configurations for entities (in Infrastructure; some configurations may live in Persistence)
- **Services/** — Implementations (e.g. AIService placeholder implementing IAIService)
- **Identity/** — Implementation of IApplicationUserStore (wraps IdentityDbContext)

### RaphCare.Persistence
- **DbContexts:** IdentityDbContext, ClinicalDbContext, DeviceDbContext, InsuranceDbContext, BillingDbContext, AIDbContext
- **Repositories/** — EfRepository<TEntity, TContext>; repository registration per entity/context in DependencyInjection
- **Configurations/** — Additional EF configurations if any
- **Seed/** — DatabaseSeeder (orchestrator), IdentitySeeder, ClinicalSeeder, InsuranceSeeder, DeviceSeeder (placeholder), BillingSeeder (placeholder)
- **Migrations/** — EF Core migrations (per DbContext); typically under Migrations folder(s)

### RaphCare.Identity
- **Entra/** — EntraOptions, EntraTokenValidator, EntraUserProvisioningService, EntraRoleMapper
- **DependencyInjection.cs** — Registers Entra options, JWT Bearer validation, user provisioning, role mapper

### RaphCare.Client
- **Contracts/** — Response<T>, ApiException
- **Generated/** — NSwag-style partial Client and IClient, generated DTOs (e.g. PatientDto, PatientDtoPagedResult)
- **Services/Base/** — BaseHttpService (wraps IClient and HttpClient; generic Get/Post/Put/Delete returning Response<T>)
- **Services/** — IPatientService, PatientService (example); inject IClient and IMapper
- **Models/** — ViewModels and request DTOs (e.g. PatientViewModel, CreatePatientRequest)
- **Mappings/** — AutoMapper profiles (DTOs to ViewModels)
- **ServiceRegistration.cs** — AddRaphCareClient (registers client, HttpClient, AutoMapper, feature services)

### RaphCare.Web
- Blazor WASM structure (App, components, pages, etc.); references RaphCare.Client

### RaphCare.Mobile
- MAUI app structure; references RaphCare.Client

## Responsibilities Summary

- **Domain:** Entity and value-object definitions only; no infrastructure
- **Application:** Orchestration via MediatR; defines interfaces consumed by Infrastructure/Persistence/Identity
- **Infrastructure:** Cross-cutting implementations (interceptors, external service adapters, user store)
- **Persistence:** Data access (DbContexts, repositories, migrations, seeding)
- **Identity:** Auth (Entra JWT validation, user provisioning, role mapping)
- **API:** HTTP surface, middleware, auth policies
- **Client:** Reusable API client for Web and Mobile
