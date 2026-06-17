# RaphCare Master Patient Index (MPI)

This document describes how the RaphCare Master Patient Index works: the patient identity architecture, global patient model, matching algorithm, creation flow, duplicate prevention, and future expansion.

---

## Overview of Patient Identity Architecture

RaphCare uses a **global patient model**: one longitudinal `Patient` record per person across all clinics. The Master Patient Index (MPI) ensures that when a patient is registered or created (e.g. from a clinic, voice onboarding, or OTP login), the system either **finds an existing patient** or **creates a new one**, avoiding duplicates.

Key components:

- **Patient** — Single aggregate root for a person’s clinical identity (demographics, links to auth, external IDs).
- **ApplicationUser** — Authenticated user (synced from Microsoft Entra ID); optionally linked to a `Patient` for “patient portal” access.
- **PatientExternalId** — Maps the global `Patient` to identifiers in external systems (clinics, insurers, labs, national registry).
- **IMasterPatientIndexService** — Service that finds a unique match by a fixed priority: National Health ID → External ID → Phone → Demographics.

Identity is **not** tied to a single clinic: the same `Patient` can have Visits and Appointments at multiple clinics, and each clinic (or other system) can have its own external ID stored in `PatientExternalIds`.

---

## Global Patient Model

The `Patient` entity (`RaphCare.Domain/Patients/Patient.cs`) is the core longitudinal record:

| Concept | Implementation |
|--------|-----------------|
| **Global ID** | `Patient.Id` (Guid) — primary key, used everywhere as the canonical patient reference. |
| **Auth link** | `ApplicationUserId` (Guid?) — optional link to the authenticated user. |
| **National identity** | `NationalHealthId` (string?) — government-issued health identifier where available. |
| **Demographics** | `FirstName`, `LastName`, `DateOfBirth`, `Gender`, `PhoneNumber`, `Email`, etc. |
| **External systems** | `ExternalIds` — collection of `PatientExternalId` (SourceSystem + ExternalId). |

Soft-deleted patients (`IsDeleted` / `DeletedAt`) are excluded from default queries via an EF Core query filter. The database enforces:

- **Unique** `NationalHealthId` (one patient per national ID).
- **Unique** `(SourceSystem, ExternalId)` on `PatientExternalIds` (one patient per external system’s ID).

---

## ApplicationUser → Patient Relationship

- **ApplicationUser** lives in the Identity bounded context (Entra-synced user; roles, sessions, audit).
- **Patient** lives in the Clinical bounded context and has an optional **ApplicationUserId** pointing to that user.

Relationship:

- **One ApplicationUser** can be linked to **at most one Patient** (conceptually; the database uses a non-unique index on `ApplicationUserId` for lookups).
- **One Patient** has **zero or one** ApplicationUser (nullable `ApplicationUserId`).

Linking is done via `Patient.LinkToApplicationUser(applicationUserId)`. It is used today mainly in the **OTP phone verification** flow: when a user verifies by phone, the system finds or creates a patient (by `ApplicationUserId` or `PhoneNumber`) and links that patient to the user. Patients created via REST or voice onboarding start with **no** ApplicationUser; they can be linked later (e.g. when the patient logs in).

---

## ExternalIds Mapping

External systems (clinics, insurers, labs, national registry) identify the same person with their own IDs. RaphCare stores these as **PatientExternalId** rows: each row ties one **Patient** to one **external (SourceSystem, ExternalId)** pair.

| Column | Meaning |
|--------|--------|
| **PatientId** | The global RaphCare patient (Guid). |
| **SourceSystem** | Logical name of the system (e.g. clinic id as string, "InsuranceProvider", "NationalRegistry"). |
| **ExternalId** | That system’s identifier for this patient. |

The composite **(SourceSystem, ExternalId)** is unique: one external ID per source points to exactly one patient.

### Example: MPI ID, Clinic ID, Insurance ID

One patient can have several external IDs:

| MPI / Meaning | SourceSystem | ExternalId |
|---------------|--------------|------------|
| RaphCare global | (none; use `Patient.Id`) | — |
| Clinic A’s chart number | `"clinic-guid-A"` or `"ClinicA"` | `"CHART-001"` |
| Clinic B’s local id | `"clinic-guid-B"` | `"LOCAL-789"` |
| Insurance member id | `"InsuranceProvider"` | `"MEM-12345"` |

When creating a patient from a clinic, the handler uses `SourceSystem = ClinicId.ToString()` and `ExternalId` from the command (or falls back to `Patient.Id`). So one global **Patient** is linked to many **ExternalIds**, each for a different source.

---

## Matching Algorithm

The MPI matching service (`MasterPatientIndexService`) finds **at most one** existing patient. It uses a **fixed priority**; the first step that returns exactly one match wins. If a step returns zero or more than one match, the next step is used (or the result is “no match” / “ambiguous”).

**Priority order:**

1. **NationalHealthId** — If provided and non-empty, find by `Patient.NationalHealthId`. If exactly one → return that patient. If multiple → treat as ambiguous (return null).
2. **ExternalId** — If **SourceSystem** and **ExternalId** are both provided, find via `PatientExternalIds` (SourceSystem + ExternalId). One matching patient → return it; more than one → ambiguous (null).
3. **Phone** — If **PhoneNumber** is provided, find by `Patient.PhoneNumber`. One match → return; multiple → ambiguous (null).
4. **Demographics** — Match on **FirstName**, **LastName**, and **DateOfBirth** (same calendar day). One match → return; multiple → ambiguous (null).

If no step yields exactly one patient, the service returns **null** (no match or ambiguous). No automatic creation happens inside the matcher; the **create-patient** flow uses this result to either reuse an existing patient or create a new one.

---

## Patient Creation Flow with MPI

The main entry point that creates patients is **CreatePatientHandler** (used by REST create and by voice onboarding via the same command).

1. **Resolve SourceSystem**  
   `SourceSystem = request.SourceSystem ?? (request.ClinicId != Guid.Empty ? request.ClinicId.ToString() : null)`.

2. **Call MPI**  
   `FindMatchAsync(NationalHealthId, SourceSystem, ExternalId, FirstName, LastName, DateOfBirth, PhoneNumber)`.

3. **If match found**  
   Return the existing patient’s `Id`; **do not create** a new patient.

4. **If no match**  
   - Create a new **Patient** (FirstName, LastName, DateOfBirth, IsActive; optional PhoneNumber, NationalHealthId).
   - If **ClinicId** is present, create a **PatientExternalId**:  
     `SourceSystem = ClinicId.ToString()`,  
     `ExternalId = request.ExternalId?.Trim() ?? patient.Id.ToString()`.
   - Save and return the new `patient.Id`.

So: **every create goes through the MPI**; creation only happens when there is no unique match. Clinic linkage is persisted via **ExternalIds**, not a column on `Patient`.

---

## Duplicate Prevention Strategy

- **At create time:** All patient creation flows that use `CreatePatientCommand` go through `CreatePatientHandler`, which always calls the MPI first. Duplicates are avoided by **reusing** the single match when one is found.
- **By identifier:**  
  - **NationalHealthId:** Unique index; MPI uses it first when present.  
  - **(SourceSystem, ExternalId):** Unique on `PatientExternalIds`; MPI uses it second.  
  - **Phone / Demographics:** No DB uniqueness; used only for matching. Multiple patients with the same phone or same name+DOB are possible if they were created before MPI or without those fields; once MPI is in use, a single match prevents a second create.
- **OTP flow:** Identity provisioning still does its own “find by ApplicationUserId / PhoneNumber” before creating a placeholder patient; that aligns with MPI (phone is in the priority list).

---

## Handling Ambiguous Matches

The matcher is **deterministic and conservative**:

- If any step (NationalHealthId, ExternalId, Phone, Demographics) returns **more than one** patient, the service **does not** pick one; it returns **null**.
- The create flow then treats null as “no match” and **creates a new patient**. So ambiguous cases can still lead to a new record until data are merged or deduplicated operationally.

Future improvements could add a “possible matches” API and a manual merge workflow so that ambiguous cases are resolved by an operator instead of creating another patient.

---

## Future Expansion

Planned or possible enhancements:

- **Probabilistic matching** — Score multiple candidates (e.g. name + DOB + address, fuzzy name) and either auto-link above a threshold or present candidates for manual review.
- **FHIR patient matching** — Align with FHIR `Patient` and the FHIR match API (e.g. `$match`) for interoperability and standard semantics.
- **National health registry integration** — Use NationalHealthId (and optionally other national IDs) to validate or sync with a national registry and keep one patient per national ID.

These would build on the current MPI (same global patient model and ExternalIds) and could add new match steps or replace the strict “exactly one or null” rule with scored/ambiguous handling.

---

## Diagrams

### ApplicationUser, Patient, and related entities

```mermaid
erDiagram
    ApplicationUser ||--o| Patient : "ApplicationUserId"
    Patient ||--o{ PatientExternalId : "ExternalIds"
    Patient ||--o{ Visit : "Visits"
    Patient ||--o{ Appointment : "Appointments"
    Visit }o--|| Clinic : "ClinicId"
    Appointment }o--|| Clinic : "ClinicId"

    ApplicationUser {
        Guid Id PK
        string EntraObjectId
        string Email
        string DisplayName
    }

    Patient {
        Guid Id PK
        Guid? ApplicationUserId FK
        string NationalHealthId
        string FirstName
        string LastName
        DateTime DateOfBirth
        string PhoneNumber
    }

    PatientExternalId {
        Guid Id PK
        Guid PatientId FK
        string SourceSystem
        string ExternalId
    }

    Visit {
        Guid Id PK
        Guid PatientId FK
        Guid ClinicId FK
        Guid AppointmentId FK
    }

    Appointment {
        Guid Id PK
        Guid PatientId FK
        Guid ClinicId FK
    }

    Clinic {
        Guid Id PK
    }
```

### Conceptual hierarchy (identity and linkages)

```
ApplicationUser
│
▼
Patient
│
├── Visits → Clinic
├── Appointments → Clinic
└── ExternalIds (PatientExternalId)
        ├── SourceSystem (e.g. ClinicId, "InsuranceProvider")
        └── ExternalId (e.g. clinic chart#, member id)
```

### MPI match priority (flow)

```
FindMatchAsync(input)
│
├─ 1. NationalHealthId provided? → Query by NationalHealthId
│       → 1 match: return Patient
│       → 0 or >1: continue
│
├─ 2. SourceSystem + ExternalId provided? → Query PatientExternalIds
│       → 1 PatientId: load Patient, return
│       → 0 or >1: continue
│
├─ 3. PhoneNumber provided? → Query by PhoneNumber
│       → 1 match: return Patient
│       → 0 or >1: continue
│
└─ 4. Demographics (FirstName, LastName, DateOfBirth) → Query
        → 1 match: return Patient
        → 0 or >1: return null
```

---

## References

- **Interface:** `RaphCare.Application/Common/Interfaces/IMasterPatientIndexService.cs`
- **Implementation:** `RaphCare.Persistence/MasterPatientIndexService.cs`
- **Patient entity:** `RaphCare.Domain/Patients/Patient.cs`
- **External IDs:** `RaphCare.Domain/Patients/PatientExternalId.cs`
- **Create flow:** `RaphCare.Application/Features/Patients/Commands/CreatePatient/CreatePatientHandler.cs`
- **Audit (historical):** `docs/Patient_Identity_And_MPI_Audit.md`
