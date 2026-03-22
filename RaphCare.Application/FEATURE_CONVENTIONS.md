# Feature Conventions

## Conventions To Reuse For Every New Feature

- Keep feature folder singular if entity is singular (`AssignedConsultant`, `CustomerService`).
- Commands use verbs: `Create`, `Update`, `Remove`.
- Queries use `FetchAll{EntityPlural}`, `FetchPaged{EntityPlural}`, `Retrieve{Entity}ById`.
- Example: `FetchPagedWorkRequestsQuery` and `FetchPagedWorkRequestsResponseDto`.
- Repository stays entity-focused (`I{Entity}Repository`) with domain-specific methods only.
- Avoid `CommandRepository` / `QueryRepository` split unless CQRS read/write storage really diverges.

## Applied Example

The `WorkRequest` feature follows this structure and naming pattern.

## Folder Conventions

- `Features/{EntitySingular}/Commands/{Action}`
- `Features/{EntitySingular}/Queries/{QueryType}`
- `Features/{EntitySingular}/Validation`
- `Features/{EntitySingular}/Mappings`
- `Features/{EntitySingular}/Shared`
- `Contracts/Persistence/I{Entity}Repository.cs` (or split query/command contracts only when needed)

For query names (`FetchAll{EntityPlural}`, `FetchPaged{EntityPlural}`, `Retrieve{Entity}ById`), use:

- `Queries/GetAll` for `FetchAll{EntityPlural}...`
- `Queries/GetPaged` for `FetchPaged{EntityPlural}...`
- `Queries/Read` for `Retrieve{Entity}ById...`

Example:

```text
Features/WorkRequest
├─ Commands
│  ├─ Create
│  ├─ Update
│  └─ Remove
├─ Queries
│  ├─ GetAll
│  ├─ GetPaged
│  └─ Read
├─ Validation
├─ Mappings
└─ Shared
```

Practical rules:

- Keep feature folder singular (`WorkRequest`, `CustomerService`).
- Keep command subfolders verb-based (`Create`, `Update`, `Remove`, plus domain actions like `Approve`).
- Keep query subfolders intent-based (`GetAll`, `GetPaged`, `Read`).
- Keep validators either in `Validation` (shared/cross-command) or next to command/query when specific.
- Keep feature mappings in `Mappings` when they are feature-specific.
- Handlers consume application DTO/command models and delegate entity transformation to AutoMapper.
- Map DTO/command -> Domain entity in mapping profiles (do not hand-map domain entities inside handlers).
- Example: `CreateWorkRequestCommand` -> `WorkRequest` is defined in `WorkRequestMappingProfile`.
- Mapping file names in `Mappings` must use `{Feature}MappingProfile.cs` (for example `WorkRequestMappingProfile.cs`).
- Add feature tests based on the `WorkRequest` test shape.
- Include command handler tests under `TCSA.Solution.UnitTests/Features/{Feature}/Commands`.
- Include query handler tests under `TCSA.Solution.UnitTests/Features/{Feature}/Queries`.
- Include validator tests under `TCSA.Solution.UnitTests/Features/{Feature}/Validators`.
- Add/update architecture convention tests to ensure each command has a handler and validator, and each handler has a corresponding test class.
- Controllers accept Web/API transport contracts only: `*RequestDto` for input and `*ResponseDto` for output.
- Controllers map DTOs to application `Command`/`Query` objects and send them via `IMediator`.
- Do not bind controller actions directly to application commands or domain entities.
- Do not put domain/business logic in controllers; keep controllers orchestration-only.
- For list endpoints, map application DTOs to response DTOs before returning.
- If a folder has only one tiny file long-term, flatten it later (do not force deep nesting).

## Optional Next Step

If needed, generate the same template for a new feature (for example `Engagement` or `CaseNote`) with pre-filled class names.
