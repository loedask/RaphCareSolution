# FHIR Interoperability (Phase 1 - FHIR-shaped exports)

## Scope
Phase 1 provides minimal, FHIR-shaped JSON exports for:
- `Patient` (global patient identity)
- `Encounter` (clinic-scoped `Visit` only)
- `Appointment` (minimal phase 1 export for existing appointments)
- `Organization` (clinic/practice)

This is **not** a full FHIR server. No full resource model, no `$everything`, and no full FHIR search semantics yet.
Phase 1 adds **minimal** `Bundle`/`searchset` collection endpoints for interoperability.

## Security
- Endpoints are restricted to `RequireProvider` (Admin/Provider).
- Patient-facing access is intentionally not exposed yet.

## Endpoints
- `GET /api/fhir/patients/{id}`
- `GET /api/fhir/patients` (minimal searchset)
- `GET /api/fhir/encounters/{id}`
- `GET /api/fhir/encounters` (minimal searchset)
- `GET /api/fhir/appointments/{id}`
- `GET /api/fhir/appointments` (minimal searchset)
- `GET /api/fhir/organizations/{id}`
- `GET /api/fhir/organizations` (minimal searchset)

## Minimal Bundle/searchset support (phase 1)
Collection endpoints return a minimal `Bundle` with:
- `Bundle.type = "searchset"`
- `Bundle.total = total matching resources (before paging)`
- `Bundle.entry[*].resource = the same minimal DTOs as the single-resource endpoints`

Supported query parameters (exact match + minimal paging):
- Patients: `id` (Guid), `nationalHealthId` (string)
- Encounters: `patientId` (Guid), `clinicId` (Guid)
- Appointments: `patientId` (Guid), `status` (string)
- Organizations: `active` (boolean)
All collection endpoints:
- `pageNumber` (int, default `1`)
- `pageSize` (int, default `20`, max `100`)

Limitations (intentional):
- No full FHIR search semantics yet (only the explicit query params above)
- No FHIR paging links/cursor semantics yet; paging is minimal/internal via `pageNumber/pageSize`
- No include/revinclude

## Mapping Rules (Identifier systems)
FHIR-shaped DTOs use internal `urn:raphcare:*` identifier systems:
- Internal patient id: `urn:raphcare:patient-id`
- `NationalHealthId`: `urn:raphcare:national-health-id`
- `NationalIdNumber`: `urn:raphcare:national-id`
- `PatientExternalIds`: `Identifier.system = PatientExternalId.SourceSystem`, `Identifier.value = PatientExternalId.ExternalId`

## Why “FHIR-shaped” (and not full FHIR yet)
- We export only the fields needed for safe interoperability today.
- This minimizes risk and avoids adding full FHIR frameworks before contract validation with EMR/insurer consumers.
- Core domain model remains unchanged; we reuse existing global patient and MPI identifiers.

## Roadmap
- TeleSession mapping
- Import endpoints (FHIR -> domain)
- Patient-safe FHIR access (clinic scoping via `PatientClinicAccess`)
- Content negotiation for `application/fhir+json`

## Appointment export limitations (phase 1)
- `Appointment.participant` includes patient actor reference only (provider/staff mapping deferred).
- `Appointment.comment` is exported from `Appointment.Reason` when available.

## Patient.birthDate format
`Patient.birthDate` is exported as an ISO date-only string in `yyyy-MM-dd` format (FHIR `date`, no time component).

