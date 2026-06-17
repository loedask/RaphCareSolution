# Coding Conventions

## Naming Conventions

- **Projects and namespaces:** Project name matches root namespace (e.g. RaphCare.Application → RaphCare.Application). Subfolders extend namespace (e.g. RaphCare.Application.Features.Patients.Commands.CreatePatient).
- **Entities:** PascalCase; singular (Patient, Visit, Appointment). Domain entities live under RaphCare.Domain with sub-namespaces by area (Identity, Organization, Patients, Clinical, etc.).
- **DTOs:** Suffix Dto (PatientDto, PatientDtoPagedResult in generated client). Commands/Queries: suffix Command or Query (CreatePatientCommand, GetPatientsQuery). Handlers: suffix Handler (CreatePatientHandler).
- **Controllers:** Suffix Controller; route `api/[controller]` (e.g. PatientsController → api/patients).
- **Interfaces:** I-prefix (IRepository, IUnitOfWork, ICurrentUserService, IAIService). Implementations: descriptive name (EfRepository, EntraUserProvisioningService).
- **Middleware:** Suffix Middleware (TenantResolutionMiddleware, ExceptionHandlingMiddleware).

## Documentation and comments

Aligned with **`.cursor/rules/raphcare-comments.mdc`** (same expectations for tooling and people).

### Generated code

- Do not add, edit, or remove XML doc comments (or block comments) by hand in **NSwag-generated** output (**`RaphCare.Client/Services/Base/NSwag/ClientService.cs`**, related generated DTOs) or other **machine-owned** artifacts. Improve descriptions via **OpenAPI/Swagger** or **NSwag** settings or templates, or accept generator output.
- **EF migrations** and similar generated files: do not hand-annotate for “documentation coverage.”

### Hand-written code

- Prefer **clear names and structure** over comments that restate the code.
- Use **`///` XML documentation** on **public** types and members when it adds value: purpose, non-obvious behavior, invariants; use **`param`**, **`returns`**, and **`exception`** (or equivalent) when callers need them. **Application** ports and **Domain** public surface benefit most.
- **Avoid** redundant `///` on trivial members, obvious properties, or boilerplate that duplicates the name.
- Use **`//`** sparingly for **why** (not **what**) when the logic is non-obvious; keep comments short and update them when code changes.

### API (HTTP surface)

- **RaphCare.API** enables XML documentation for the host assembly: prefer accurate **OpenAPI** via **`ProducesResponseType`**, action **`Name`**, and Swagger-friendly DTOs. Optional `///` on controllers when it helps IDE users; **OpenAPI** is the contract for **RaphCare.Client** consumers.

### Pull requests and refactors

- Avoid change sets whose only goal is blanket comment coverage. Do not hand-edit **NSwag** client files to add XML documentation.

## Dependency Injection Patterns

- **Registration:** Each layer exposes a static `DependencyInjection` class (or similar) with extension method(s) on IServiceCollection (AddApplication, AddPersistence, AddIdentity, AddInfrastructure, AddRaphCareClient). API Program.cs calls these in order: AddApplication → AddInfrastructure → AddPersistence → AddIdentity → AddControllers, then auth and Swagger.
- **Lifetimes:** Handlers and repositories are registered per-context (Scoped or Transient as appropriate). DbContexts are Scoped. Options (EntraOptions) are Configure from configuration. Validators and behaviors are typically Transient.
- **Resolving DbContexts:** Handlers receive IRepository<T> or specific services; repositories are registered per entity and DbContext (e.g. EfRepository<Patient, ClinicalDbContext>). No direct DbContext in controllers; MediatR only.

## Layer Separation Rules

- **Domain:** No references to Application, Infrastructure, Persistence, or API. Only domain types and interfaces in Domain.Common.
- **Application:** References Domain only. Defines interfaces (IRepository, IUnitOfWork, IAIService, IOtpService, ITokenService, ISpeechToTextService, etc.); no concrete persistence or external service implementations. Application logic lives in MediatR handlers and validators.
- **Infrastructure:** Implements application interfaces (e.g. AIService, interceptors, CurrentUserService, DateTimeProvider, placeholder services). References Application and Domain. May reference Persistence for DbContext types if implementing store against them. IApplicationUserStore is implemented in Persistence, not Infrastructure.
- **Persistence:** DbContexts, configurations, repositories, migrations, seeders, ApplicationUserStore (IApplicationUserStore). References Domain and Application (interfaces). No reference to API or Identity.
- **Identity:** Auth and user provisioning. References Application (IUserProvisioningService, IApplicationUserStore contract) and Domain (ApplicationUser). No reference to Persistence; user store implementation lives in Persistence (ApplicationUserStore).
- **API:** References Application, Infrastructure, Persistence, Identity. No business logic; controllers send MediatR requests and return results. Middleware for cross-cutting concerns (exception, tenant, audit).

## Folder Organization Standards

- **Application Features:** One folder per feature (e.g. Patients, Appointments). Under each: Commands and Queries with subfolders per use case (CreatePatient, GetPatientById). Each use case folder contains Command/Query, Handler, optional Validator, optional DTOs.
- **Domain:** One folder (and namespace) per bounded area; entities and value objects in that namespace. Common for shared base types and interfaces.
- **Persistence:** DbContexts at root; Repositories in Repositories/; Seed in Seed/; migrations in Migrations/ (or per-context subfolders). Configurations can be in Persistence or Infrastructure.Persistence.Configurations.
- **API:** Controllers in Controllers/; middleware in App/Middleware/; extensions in App/Extensions/; API contracts (e.g. response DTOs for Swagger) in App/Contracts/.
- **Client:** Contracts (Response, ApiException, IAccessTokenProvider), Contracts/Interfaces (IPatientService), Services/Base (IClient, Client, BaseHttpService, BearerTokenHandler), Services/Base/Generated (NSwag ClientService), Services, Models, Mappings; one ServiceRegistration for registration. When using bearer auth, host (e.g. Mobile) must register IAccessTokenProvider; AddRaphCareClient(useBearerToken: true) adds BearerTokenHandler.
- **Mobile:** Put feature-specific UI and VMs in **`Core/Features/<Feature>/`**; put cross-feature reuse in **Core/Shared/**; put composition/DI glue in **Core/Infrastructure/**. ViewModels use `RaphCare.Mobile.Core.Features.*.ViewModels`; Views use `RaphCare.Mobile.Core.Features.*.Views`. Shared MAUI controls: **Core/Shared/Controls** (`RaphCare.Mobile.Core.Shared.Controls`). Blazor hybrid: **Blazor/** (`RaphCare.Mobile.Blazor`) and **Core/Features/Hybrid/** (`BlazorHostPage`). **RaphCare.Mobile.Kernel** holds `AuthResult` and feature-flag types (no MAUI) for sharing with **RaphCare.Mobile.Tests**. API access and DTOs stay in **RaphCare.Client** only. Full layout, config, secrets, and `dotnet test`: **docs/09_Mobile_App_Guide.md**.
