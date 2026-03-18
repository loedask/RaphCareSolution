# FHIR Mapping Matrix (Phase 1 - FHIR-shaped exports)

This document lists the mapping rules used by the **phase 1** FHIR-shaped export layer.
The goal is **interoperability-ready JSON exports**, not a complete FHIR server implementation.

Format: `RaphCare field -> FHIR field` with notes.

---

## Patient mapping

| RaphCare field | FHIR field | Notes |
|---|---|---|
| `Patient.Id` | `Patient.identifier` (system = `urn:raphcare:patient-id`) | Internal patient id |
| `Patient.NationalHealthId` | `Patient.identifier` (system = `urn:raphcare:national-health-id`) | Included only when not null/empty |
| `Patient.NationalIdNumber` | `Patient.identifier` (system = `urn:raphcare:national-id`) | Included only when not null/empty |
| `PatientExternalIds[*].ExternalId` + `PatientExternalIds[*].SourceSystem` | `Patient.identifier[*]` | One identifier per `PatientExternalId` |
| `Patient.FirstName` + `Patient.LastName` | `Patient.name` (`HumanName.given` + `HumanName.family`) | Given = first name, Family = last name (one HumanName element) |
| `Patient.Gender` | `Patient.gender` | Included only when non-empty |
| `Patient.DateOfBirth` | `Patient.birthDate` | Exported as date-only string `yyyy-MM-dd` (FHIR `date`) |
| `Patient.PhoneNumber` | `Patient.telecom` (`ContactPoint.system="phone"`) | Included only when non-empty |
| `Patient.Email` | `Patient.telecom` (`ContactPoint.system="email"`) | Included only when non-empty |

---

## Encounter mapping

Phase 1 uses **`Visit`** as the canonical source for FHIR `Encounter`.

| RaphCare field | FHIR field | Notes |
|---|---|---|
| `Visit.Id` | `Encounter.id` |  |
| `Visit.Status` | `Encounter.status` | Included only when non-empty |
| `Visit.PatientId` | `Encounter.subject.reference` | `Patient/{Visit.PatientId}` |
| `Visit.ClinicId` | `Encounter.serviceProvider.reference` | `Organization/{Visit.ClinicId}` |
| `Visit.VisitStart` | `Encounter.period.start` | Period start included when available (phase 1 DTO stores `DateTime?`) |
| `Visit.VisitEnd` | `Encounter.period.end` | Optional period end |
| `Visit.AppointmentId` | `Encounter.appointment.reference` | Included only when `AppointmentId != Guid.Empty` |

---

## Appointment mapping

Phase 1 exports existing **`Appointment`** as a minimal FHIR-shaped `Appointment` DTO.

| RaphCare field | FHIR field | Notes |
|---|---|---|
| `Appointment.Id` | `Appointment.id` |  |
| `Appointment.Status` | `Appointment.status` | Included only when non-empty |
| `Appointment.ScheduledStart` | `Appointment.start` |  |
| `Appointment.ScheduledEnd` | `Appointment.end` |  |
| `Appointment.PatientId` | `Appointment.participant.actor.reference` | `Patient/{Appointment.PatientId}` (phase 1 includes patient participant only) |
| `Appointment.Status` | `Appointment.participant.status` | Uses same value as `Appointment.Status` in phase 1 |
| `Appointment.Reason` | `Appointment.comment` | Exported when non-empty |
| `Appointment.ProviderId` | `Appointment.participant.actor.reference` | Deferred (phase 2). No clean Practitioner mapping exists yet in phase 1. |
| `Appointment.ClinicId` | (not mapped) | Deferred (phase 2). |

## Organization mapping

Phase 1 maps **`Clinic`** to FHIR `Organization`.

| RaphCare field | FHIR field | Notes |
|---|---|---|
| `Clinic.Id` | `Organization.id` |  |
| `Clinic.Name` | `Organization.name` | Included only when non-empty |
| `Clinic.RegistrationNumber` | `Organization.identifier` (system = `urn:raphcare:clinic-registration-number`) | Included only when non-empty |
| `Clinic.IsActive` | `Organization.active` | Exported as `bool?` |

---

## Bundle searchset (collection endpoints)

Phase 1 collection endpoints return a minimal `Bundle`:
- `Bundle.type = "searchset"`
- `Bundle.total = number of returned resources`
- `Bundle.entry[*].resource` uses the exact same minimal DTO mappings as the corresponding single-resource endpoints (`Patient`, `Encounter`, `Appointment`, `Organization`)
- `Bundle.entry[*].fullUrl` uses a stable URN: `urn:raphcare:fhir:{ResourceType}/{id}`

Notes:
- No paging/cursor semantics yet
- No include/revinclude
- No full FHIR search semantics beyond the explicitly supported exact-match query parameters

