# FHIR Interoperability (Phase 1 - FHIR-shaped exports)

## Scope
Phase 1 provides minimal, FHIR-shaped JSON exports for:
- `Patient` (global patient identity)
- `Encounter` (clinic-scoped `Visit` only)
- `Organization` (clinic/practice)

This is **not** a full FHIR server. No bundles/search endpoints, no full resource model, and no FHIR `$everything` support yet.

## Security
- Endpoints are restricted to `RequireProvider` (Admin/Provider).
- Patient-facing access is intentionally not exposed yet.

## Endpoints
- `GET /api/fhir/patients/{id}`
- `GET /api/fhir/encounters/{id}`
- `GET /api/fhir/organizations/{id}`

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
- Appointment mapping
- TeleSession mapping
- Bundle export/search support
- Import endpoints (FHIR -> domain)
- Patient-safe FHIR access (clinic scoping via `PatientClinicAccess`)
- Content negotiation for `application/fhir+json`

## Patient.birthDate format
`Patient.birthDate` is exported as an ISO date-only string in `yyyy-MM-dd` format (FHIR `date`, no time component).

