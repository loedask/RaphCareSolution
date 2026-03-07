# RaphCare System Overview

## High-Level Description

RaphCare is a healthcare platform supporting clinical workflows, patient management, appointments, visits, telemedicine, device monitoring, insurance profiles, billing, mental health assessments, reporting, and AI-assisted summaries. The system is multi-tenant at the API level (clinic-scoped via `X-Clinic-Id` header) and uses a bounded-context architecture with separate DbContexts per domain area.

## Tech Stack

- **Runtime:** .NET 10
- **API:** ASP.NET Core (REST, JSON)
- **Data:** Entity Framework Core 9, SQL Server
- **Application layer:** MediatR (CQRS-style), FluentValidation
- **Auth:** JWT Bearer; tokens validated against Microsoft Entra ID (Azure AD)
- **Client:** Blazor WebAssembly (RaphCare.Web), .NET MAUI Blazor Hybrid (RaphCare.Mobile)
- **Shared client:** RaphCare.Client (HTTP client abstraction, NSwag-style generated client, AutoMapper)

## Architectural Style

- **Layered / vertical slices:** Domain → Application (features as Commands/Queries/Handlers) → Infrastructure → Persistence → API
- **Bounded contexts:** Identity, Clinical, Devices, Insurance, Billing, AI each have their own DbContext and connection string option
- **CQRS:** Application use cases are implemented as MediatR requests (commands and queries) with handlers; no application services in the traditional sense
- **Repository pattern:** Generic `IRepository<T>` implemented per entity/DbContext via `EfRepository<TEntity, TContext>`
- **Unit of work:** Persistence layer exposes coordination for multi-context saves where needed

## Deployment Model

- **API:** Single deployable (ASP.NET Core host). Migrations run automatically in Development only via `ApplyMigrationsAsync` at startup.
- **Databases:** One or more SQL Server databases; each bounded context can use a dedicated connection string (e.g. DefaultConnection, IdentityConnection, ClinicalConnection, DeviceConnection, InsuranceConnection) or share one.
- **Clients:** Blazor WASM and MAUI consume the API via RaphCare.Client; base URL and auth are configured at host level (no hardcoded base URL in client).
