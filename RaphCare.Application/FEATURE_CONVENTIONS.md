# RaphCare.Application — feature conventions

Conventions for **RaphCare.Application** (MediatR, features, shared `Common/`). Cursor rule: **`.cursor/rules/raphcare-application.mdc`**.

## Folder layout

- **`Features/{Feature}/Commands/{Action}/`** — `{Action}Command`, `{Action}Handler`, `{Action}Validator` (validator typical for writes).
- **`Features/{Feature}/Queries/{QueryName}/`** — `{QueryName}Query`, `{QueryName}Handler`; namespace matches folders (e.g. `RaphCare.Application.Features.Patients.Queries.GetPatients`).
- **`Features/{Feature}/DTOs/`** — Feature DTOs returned by queries or shared within the feature.
- **`Common/Behaviors`**, **`Common/Interfaces`**, **`Common/DTOs`**, etc. — Cross-cutting pipeline behaviors, abstractions (`IRepository<T>`, `IUnitOfWork`), **`PagedResult<T>`**, security helpers.

Handlers use **`IRepository<TEntity>`** from **`Common/Interfaces`** (implemented in Persistence)—not per-entity repository interfaces in Application unless you introduce them for a specific bounded context.

## Naming

- **Feature folders:** Match existing areas (`Patients`, `Appointments`, `Auth`, `Billing`, `Clinical`, …).
- **Commands:** `Create*`, `Update*`, `Remove*` for CRUD; domain verbs otherwise (`SendOtp`, `VerifyOtp`, `GenerateSummary`).
- **Queries:** **`Get{Plural}`** for collections (often paged), **`Get{Singular}ById`** for one entity—e.g. `GetPatientsQuery`, `GetPatientByIdQuery`, `GetAppointmentsQuery`.

## Validation and mapping

- Colocate **`*Validator`** with the command when it is operation-specific.
- Use **FluentValidation**; register validators with the same assembly MediatR uses.
- Prefer explicit mapping in handlers or centralized profiles only where the codebase already does; many features rely on feature **DTOs** and straightforward projections.

## API (RaphCare.API)

- Controllers call **`IMediator`** with application **commands/queries**.
- This solution often binds **`[FromBody]`** directly to **`*Command`** types—acceptable and established; optional `*RequestDto` is not required unless you add a dedicated API contract layer.
- Keep controllers thin: no business logic.

## Pragmatic rules

- If a folder holds a single small file long-term, consider flattening—do not add depth for its own sake.
- New features should mirror **Patients** or **Appointments** structure unless there is a strong reason to differ.

## Tests

- Add unit tests in a dedicated test project when present, mirroring **`Features/{Feature}/...`**. Naming: `*HandlerTests`, `*ValidatorTests`.
