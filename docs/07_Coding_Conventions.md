# Coding Conventions

## Naming Conventions

- **Projects and namespaces:** Project name matches root namespace (e.g. RaphCare.Application → RaphCare.Application). Subfolders extend namespace (e.g. RaphCare.Application.Features.Patients.Commands.CreatePatient).
- **Entities:** PascalCase; singular (Patient, Visit, Appointment). Domain entities live under RaphCare.Domain with sub-namespaces by area (Identity, Organization, Patients, Clinical, etc.).
- **DTOs:** Suffix Dto (PatientDto, PagedResultOfPatientDto). Commands/Queries: suffix Command or Query (CreatePatientCommand, GetPatientsQuery). Handlers: suffix Handler (CreatePatientHandler).
- **Controllers:** Suffix Controller; route `api/[controller]` (e.g. PatientsController → api/patients).
- **Interfaces:** I-prefix (IRepository, IUnitOfWork, ICurrentUserService, IAIService). Implementations: descriptive name (EfRepository, EntraUserProvisioningService).
- **Middleware:** Suffix Middleware (TenantResolutionMiddleware, ExceptionHandlingMiddleware).

## Dependency Injection Patterns

- **Registration:** Each layer exposes a static `DependencyInjection` class (or similar) with extension method(s) on IServiceCollection (AddApplication, AddPersistence, AddIdentity, AddInfrastructure, AddRaphCareClient). API Program.cs calls these in order: AddApplication → AddInfrastructure → AddPersistence → AddIdentity → AddControllers, then auth and Swagger.
- **Lifetimes:** Handlers and repositories are registered per-context (Scoped or Transient as appropriate). DbContexts are Scoped. Options (EntraOptions) are Configure from configuration. Validators and behaviors are typically Transient.
- **Resolving DbContexts:** Handlers receive IRepository<T> or specific services; repositories are registered per entity and DbContext (e.g. EfRepository<Patient, ClinicalDbContext>). No direct DbContext in controllers; MediatR only.

## Layer Separation Rules

- **Domain:** No references to Application, Infrastructure, Persistence, or API. Only domain types and interfaces in Domain.Common.
- **Application:** References Domain only. Defines interfaces (IRepository, IUnitOfWork, IAIService, etc.); no concrete persistence or external service implementations. Application logic lives in MediatR handlers and validators.
- **Infrastructure:** Implements application interfaces (e.g. AIService, interceptors, IApplicationUserStore). References Application and Domain. May reference Persistence for DbContext types if implementing store against them.
- **Persistence:** DbContexts, configurations, repositories, migrations, seeders. References Domain and Application (interfaces). No reference to API or Identity.
- **Identity:** Auth and user provisioning. References Application (IUserProvisioningService, IApplicationUserStore contract) and Domain (ApplicationUser). No reference to Persistence; user store implementation lives in Infrastructure.
- **API:** References Application, Infrastructure, Persistence, Identity. No business logic; controllers send MediatR requests and return results. Middleware for cross-cutting concerns (exception, tenant, audit).

## Folder Organization Standards

- **Application Features:** One folder per feature (e.g. Patients, Appointments). Under each: Commands and Queries with subfolders per use case (CreatePatient, GetPatientById). Each use case folder contains Command/Query, Handler, optional Validator, optional DTOs.
- **Domain:** One folder (and namespace) per bounded area; entities and value objects in that namespace. Common for shared base types and interfaces.
- **Persistence:** DbContexts at root; Repositories in Repositories/; Seed in Seed/; migrations in Migrations/ (or per-context subfolders). Configurations can be in Persistence or Infrastructure.Persistence.Configurations.
- **API:** Controllers in Controllers/; middleware in Middleware/; extensions in Extensions/.
- **Client:** Contracts, Generated, Services, Services/Base, Models, Mappings; one ServiceRegistration for registration.
