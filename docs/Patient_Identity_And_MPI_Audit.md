# Patient Identity and Deduplication Architecture Audit

**Purpose:** Understand how patient identities are created, linked, and matched across clinics so we can design a Master Patient Index (MPI) without duplicating existing logic.

**Audit date:** March 2025

---

## 1. Patient Identity Model

### 1.1 Patient Entity Structure

**Location:** `RaphCare.Domain/Patients/Patient.cs`

| Field | Type | Notes |
|-------|------|--------|
| **Id** | `Guid` | From `AggregateRoot` → `BaseEntity`; set in constructor via `Guid.NewGuid()`. |
| **ApplicationUserId** | `Guid?` | Nullable. Link to auth user; "legacy/manual patients without an account yet" supported. Set only via `LinkToApplicationUser()`. |
| **NationalHealthId** | `string?` | Optional; "national or government-issued health identifier". Set only via `SetNationalHealthId()`. |
| **FirstName** | `string` | Required in EF (max 100). |
| **LastName** | `string` | Required in EF (max 100). |
| **DateOfBirth** | `DateTime` | Required. |
| **Gender** | `string` | Required in EF (max 20). |
| **NationalIdNumber** | `string?` | Optional (max 50). |
| **PhoneNumber** | `string?` | Optional (max 30). |
| **Email** | `string?` | Optional (max 256). |
| **IsActive** | `bool` | |
| **IsDeleted** / **DeletedAt** | `bool` / `DateTime?` | Soft delete (ISoftDelete). |
| **ExternalIds** | `ICollection<PatientExternalId>` | Navigation to external system identifiers. |

Related aggregates/navigation: PatientProfile, Addresses, EmergencyContacts, ConsentRecords, InsuranceProfiles, MedicalHistories, Allergies, ChronicConditions, Immunizations, Medications, FamilyHistories, LifestyleProfile, RiskFactors, PatientTags, VoiceRecordings, **ExternalIds**.

### 1.2 Indexes and Matching

**Configuration:** `RaphCare.Infrastructure/Persistence/Configurations/PatientConfiguration.cs`

- **ApplicationUserId:** `HasIndex(e => e.ApplicationUserId)` — non-unique (supports lookup by user).
- **NationalHealthId:** `HasIndex(e => e.NationalHealthId)` — non-unique.
- **Query filter:** `HasQueryFilter(e => !e.IsDeleted)` — soft-deleted patients excluded by default.

No index on (FirstName, LastName, DateOfBirth), PhoneNumber, or Email. No unique constraint on NationalHealthId or any demographic set.

**Conclusion:** Patient is structured for global identity (ApplicationUserId, NationalHealthId, ExternalIds) but **no database-level uniqueness** on national ID or demographics and **no matching indexes** beyond ApplicationUserId and NationalHealthId.

---

## 2. PatientExternalId Usage

### 2.1 Entity and Table

**Location:** `RaphCare.Domain/Patients/PatientExternalId.cs`

- **PatientId** (Guid), **SourceSystem** (string, max 256), **ExternalId** (string, max 256).
- Documented as: "External system identifier for a global patient record (e.g., clinic, insurer, lab, national registry)."

**Configuration:** `RaphCare.Infrastructure/Persistence/Configurations/PatientExternalIdConfiguration.cs`

- Table: `PatientExternalIds`.
- Index: composite `(SourceSystem, ExternalId)` — **non-unique** (no `.IsUnique()`).

### 2.2 Where It Is Created

- **No application code creates or adds `PatientExternalId` records.**  
- Grep for `new PatientExternalId`, `ExternalIds.Add`, `AddAsync.*PatientExternalId` returns only configuration/migration references, no creation logic.

### 2.3 SourceSystem Population

- **No service or handler sets `SourceSystem` or `ExternalId`.**  
- The table and navigation exist and are ready for use, but no integration or registration flow writes to them.

### 2.4 Use for Lookup When Creating Patients

- **No lookup by external id or source system.**  
- No patterns such as "find patient by external id", "find by source system", or `AnyAsync(e => e.SourceSystem ...)` in the solution.

**Conclusion:** PatientExternalId is **defined and migrated but unused**. No creation, no SourceSystem population, and no use in patient creation or matching. Ideal place to plug MPI/clinic identifiers without duplicating logic.

---

## 3. Patient Creation Flow

### 3.1 Flows That Create a Patient

| Flow | Entry | Handler / Service | Creates Patient? |
|------|--------|-------------------|-------------------|
| **REST create** | POST api/patients (body: CreatePatientCommand) | `CreatePatientHandler` | Yes |
| **Voice onboarding** | POST api/onboarding/voice (multipart) | `CreatePatientFromVoiceHandler` → sends `CreatePatientCommand` via MediatR | Yes (via same command) |
| **OTP verify (phone login)** | POST api/auth/otp/verify | `VerifyOtpHandler` → `IIdentityOtpProvisioningService.EnsureUserAndPatientForPhoneAsync` | Yes, only if no patient found by ApplicationUserId or PhoneNumber |

### 3.2 CreatePatientCommand and Handler

**Command:** `RaphCare.Application/Features/Patients/Commands/CreatePatient/CreatePatientCommand.cs`

- **Required (and validated):** `ClinicId` (NotEmpty), `FirstName`, `LastName` (NotEmpty, max 100), `DateOfBirth` (LessThan UtcNow).
- **Not in command:** NationalHealthId, PhoneNumber, Email, Gender, NationalIdNumber, ApplicationUserId, ExternalIds.

**Handler:** `CreatePatientHandler.cs`

- Builds `Patient` with: FirstName, LastName, DateOfBirth, IsActive = true.
- **Does not set:** ApplicationUserId, NationalHealthId, Gender, PhoneNumber, Email, ClinicId (column removed from Patient by migration).
- **No duplicate check** — always adds a new patient.
- **ClinicId** is validated and sent in the command but **never persisted** (Patient no longer has ClinicId after `GlobalPatientIdentity` migration).

### 3.3 CreatePatientFromVoiceHandler

- Extracts FirstName, LastName, DateOfBirth from voice transcription (defaults: "Unknown", "Unknown", 1990-01-01).
- Sends `CreatePatientCommand` (same as above) → same no-duplicate behavior.
- After create: sets `patient.PhoneNumber = request.PhoneNumber` and creates a `VoiceRecording`.

**Conclusion:**  
- **Required at create:** FirstName, LastName, DateOfBirth (and ClinicId in the API only; not stored on Patient).  
- **No duplicate checks** in either REST or voice flow: no match on NationalHealthId, demographics (name + DOB), phone, or email.  
- **NationalHealthId and demographics are not used for matching** in creation.

---

## 4. Identity Linking (Patient ↔ ApplicationUser)

### 4.1 LinkToApplicationUser

- **Domain:** `Patient.LinkToApplicationUser(Guid applicationUserId)` — sets `ApplicationUserId`; throws if Guid.Empty.
- **Usage:** Only in `IdentityOtpProvisioningService.EnsureUserAndPatientForPhoneAsync` (see below).

### 4.2 ApplicationUserId Assignment

- **CreatePatientHandler** and **CreatePatientFromVoiceHandler** do **not** set ApplicationUserId.  
- **IdentityOtpProvisioningService** (OTP verify path):
  1. Find or create `ApplicationUser` by Email (used to store normalized phone).
  2. Find patient by `ApplicationUserId == user.Id`.
  3. If none: find patient by `PhoneNumber == normalizedPhone` (backfill).
  4. If still none: create a new Patient (placeholder FirstName = phone, empty LastName, DOB = UtcNow), then link.
  5. Call `patient.LinkToApplicationUser(user.Id)` and save.

So **ApplicationUserId is set only when a user verifies OTP**, either for an existing patient (by user id or phone) or for a newly created placeholder patient.

### 4.3 Can a Patient Exist Without ApplicationUserId?

- **Yes.** Domain and migration define `ApplicationUserId` as nullable.  
- All creation flows (REST, voice) create patients **without** ApplicationUserId.  
- Only the OTP verify flow links a patient to an user; until then the patient has no linked identity user.

**Conclusion:** Patients can exist without ApplicationUserId. Linking is optional and happens at phone OTP verification (find-or-create by phone, then link).

---

## 5. Deduplication Logic

### 5.1 Existing “Duplicate” Avoidance

- **Phone-based (OTP flow only):**  
  `IdentityOtpProvisioningService` avoids creating a **second** patient for the same phone by:
  - First looking up by `ApplicationUserId`.
  - Then by `PhoneNumber == normalizedPhone`.  
  If a match is found, it links that patient to the user instead of creating a new one.  
  So **same phone → one patient per phone** for the OTP flow only.

### 5.2 No Other Duplicate Prevention

- **No** “find patient by email”.
- **No** “find patient by national ID” or NationalHealthId.
- **No** “find patient by name + DOB” or similar demographic matching.
- **No** duplicate check in `CreatePatientHandler` or `CreatePatientFromVoiceHandler`.

**Conclusion:** The only deduplication is **phone-based**, and only in the OTP provisioning path. There is no demographic-, national-ID-, or external-ID-based duplicate prevention. Multiple patients with the same name/DOB (or same NationalHealthId) can be created.

---

## 6. External System Mapping

### 6.1 SourceSystem and Integrations

- **PatientExternalId** is intended for “clinic, insurer, lab, national registry” (see entity comment).  
- **No code** sets SourceSystem or ExternalId.  
- **No** integration services, sync jobs, or APIs that:
  - Write to PatientExternalIds,
  - Or read by SourceSystem/ExternalId for clinics, insurance, labs, or device vendors.

### 6.2 Naming and Conventions

- No defined SourceSystem naming convention or constants in the codebase.  
- Table and index support a future convention (e.g. per-clinic or per-system identifiers) but are currently unused.

**Conclusion:** ExternalIds are not used for any external system mapping today. They are the right place to add clinic/MPI identifiers and integration keys later.

---

## 7. Database Constraints

### 7.1 Migrations Reviewed

- **InitialClinical** (20260307180023): Patients had ClinicId (and indexes).  
- **GlobalPatientIdentity** (20260316120000):  
  - Dropped ClinicId (and its indexes) from Patients.  
  - Added nullable ApplicationUserId, nullable NationalHealthId.  
  - Created IX_Patients_ApplicationUserId, IX_Patients_NationalHealthId (non-unique).  
  - Created **PatientExternalIds** with FK to Patients, and index on (SourceSystem, ExternalId) — **not unique**.

### 7.2 Uniqueness Summary

| Element | Unique? | Location |
|--------|---------|----------|
| Patient.Id | Yes (PK) | Default |
| ApplicationUserId on Patient | No | Index only |
| NationalHealthId | No | Index only |
| (SourceSystem, ExternalId) on PatientExternalIds | No | Index only |
| Patient + DOB / name+DOB | No | No such index or constraint |

Other entities (e.g. Clinic RegistrationNumber, Device SerialNumber, Insurance Code) use `.IsUnique()` where needed; Patient and PatientExternalId do not.

**Conclusion:** No unique constraints on NationalHealthId or (SourceSystem, ExternalId). Duplicate national IDs and duplicate external ids per source system are allowed at the database level.

---

## 8. Summary and MPI-Relevant Gaps

### 8.1 Current Patient Identity Structure

- **Identity:** Id (Guid), optional ApplicationUserId (set at OTP verify), optional NationalHealthId (setter exists, never called in app code).
- **Demographics:** FirstName, LastName, DateOfBirth, Gender, NationalIdNumber, PhoneNumber, Email (all optional except name/DOB in creation).
- **External keys:** PatientExternalIds (SourceSystem + ExternalId) exist in schema and domain but are **never created or queried**.

### 8.2 Duplicate Prevention

- **Only** phone-based in OTP flow (one patient per phone when logging in).
- **No** duplicate prevention in REST or voice patient creation; no matching on NationalHealthId or demographics.

### 8.3 Use of ExternalIds for Matching

- **Not used.** No creation, no lookup by external id or source system. Ready for MPI/clinic identifiers.

### 8.4 NationalHealthId

- **Not enforced:** Optional, indexed but not unique. **Never set** in application code (only `SetNationalHealthId` exists). No matching on it.

### 8.5 Gaps That Affect an MPI Design

1. **No global match at create:** Creating a patient (REST or voice) does not check for an existing patient by national ID, external id, or demographics. An MPI layer would need to be invoked **before** or **inside** create to enforce “one identity per person”.
2. **ClinicId in command only:** CreatePatientCommand has ClinicId but it is not stored on Patient. If “patient at clinic” is represented via ExternalIds (e.g. SourceSystem = clinic id/name, ExternalId = clinic’s local id), that logic does not exist yet; MPI could own it.
3. **No unique constraints:** To support “one record per national ID” or “one record per (SourceSystem, ExternalId)”, you will need unique index(es) and/or application-level rules; today the DB allows duplicates.
4. **NationalHealthId unused:** No reads or writes. MPI can define when and how to set it and use it for matching.
5. **Single linking path:** Only OTP-by-phone links Patient to ApplicationUser. MPI does not conflict with this; MPI can sit alongside for demographic/external-id matching.
6. **Repository is generic:** Patient is loaded by Id or full list only (no specialized “find by national id” or “find by external id”). MPI will need new query methods or a dedicated store for match logic.

---

## 9. Recommendations for MPI Design

- **Use PatientExternalIds** for clinic and system identifiers: define SourceSystem (e.g. clinic code or “MPI”) and ExternalId (e.g. local patient id or MPI id), and add creation/lookup in registration or create flows.
- **Introduce match step before create:** In CreatePatient (and optionally CreatePatientFromVoice), call an MPI/match service that can:
  - Look up by NationalHealthId (when present),
  - Look up by (SourceSystem, ExternalId) when creating from a specific system,
  - Optionally match on demographics (e.g. name + DOB) with clear policy (e.g. manual review or auto-link).
- **Add unique constraints** where business rules require one patient per national ID or per (SourceSystem, ExternalId), and back them with application logic that assigns/updates NationalHealthId and ExternalIds.
- **Keep OTP linking as-is:** Continue using phone in IdentityOtpProvisioningService for “same phone → same patient”; MPI can later also consider phone as a match key if desired.
- **Clarify ClinicId:** Either remove it from CreatePatientCommand or persist it via ExternalIds (e.g. one PatientExternalId per clinic the patient is registered at) so clinic-scoping is consistent with the global Patient model.

This audit gives a complete picture of the current patient identity and deduplication behavior and where to plug in an MPI layer for global patient matching across clinics.
