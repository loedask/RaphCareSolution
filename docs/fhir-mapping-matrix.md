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
| `Patient.DateOfBirth` | `Patient.birthDate` | Exported as date-time (DTO uses `DateTime?`) |
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

## Organization mapping

Phase 1 maps **`Clinic`** to FHIR `Organization`.

| RaphCare field | FHIR field | Notes |
|---|---|---|
| `Clinic.Id` | `Organization.id` |  |
| `Clinic.Name` | `Organization.name` | Included only when non-empty |
| `Clinic.RegistrationNumber` | `Organization.identifier` (system = `urn:raphcare:clinic-registration-number`) | Included only when non-empty |
| `Clinic.IsActive` | `Organization.active` | Exported as `bool?` |

