# Master Patient Index (MPI) – Patient Creation & Identity Resolution Audit

**Date:** 2025-03-17  
**Goal:** Ensure all patient creation flows use `IMasterPatientIndexService` so duplicate patient records cannot be created.

---

## Executive Summary

| Area | Status | Notes |
|------|--------|--------|
| Patient creation paths | ⚠️ Partial | One path bypasses MPI (OTP provisioning). |
| Registration flows | ⚠️ Partial | CreatePatient and Voice use MPI; OTP does not. |
| External ID mapping | ✅ / ⚠️ | CreatePatient creates `PatientExternalId` correctly; Voice uses fallback; OTP creates no external ID. |
| Unique constraint safety | ❌ Missing | No `DbUpdateException` handling for duplicate NationalHealthId or (SourceSystem, ExternalId). |
| Repository queries | ✅ In MPI | Find by NationalHealthId and (SourceSystem, ExternalId) implemented in `MasterPatientIndexService` using indexed columns; no separate repository API. |

---

## 1. Patient Creation Paths

### 1.1 Locations Where a Patient Entity Is Created

| Location | Code Pattern | MPI Used? | Notes |
|----------|--------------|-----------|--------|
| **CreatePatientHandler** | `new Patient { ... }` then `_repository.AddAsync(patient)` | ✅ Yes | Calls `_mpi.FindMatchAsync(...)` before creating; returns existing Id when match found. |
| **IdentityOtpProvisioningService** | `new Patient { ... }` then `_clinicalDbContext.Patients.AddAsync(patient)` | ❌ No | Does not use `IMasterPatientIndexService`. Only checks `ApplicationUserId` and `PhoneNumber`. |

**CreatePatientFromVoiceHandler** does **not** create a `Patient` directly. It builds a `CreatePatientCommand` and sends it via `_mediator.Send(createPatient, cancellationToken)`, so patient creation is done by **CreatePatientHandler**, which uses MPI.

### 1.2 Unsafe Creation Paths

- **IdentityOtpProvisioningService.EnsureUserAndPatientForPhoneAsync**  
  Creates a new `Patient` when no patient is found by `ApplicationUserId` or `PhoneNumber`. It does **not** call `IMasterPatientIndexService.FindMatchAsync`.  
  **Risk:** If the same phone was already used to create a patient via REST or voice onboarding, MPI would have matched on phone and returned that patient. The OTP path can still create a **second** patient for the same phone if that existing patient has no `ApplicationUserId` set (e.g. created before OTP linking). Duplicate patients are possible.

---

## 2. Registration Flows

### 2.1 CreatePatientHandler

- **MPI usage:** ✅ Yes.  
- Calls `_mpi.FindMatchAsync(NationalHealthId, sourceSystem, ExternalId, FirstName, LastName, DateOfBirth, PhoneNumber)` before creating.  
- If a single match is found, returns `existing.Id` and does not create a new patient.

### 2.2 CreatePatientFromVoiceHandler

- **MPI usage:** ✅ Yes (via CreatePatientHandler).  
- Sends `CreatePatientCommand` with: `ClinicId`, `FirstName`, `LastName`, `DateOfBirth`, `PhoneNumber`, `SourceSystem = request.ClinicId.ToString()`.  
- Does **not** set `NationalHealthId` or `ExternalId` (voice does not extract them).  
- MPI still runs with phone + demographics + SourceSystem (no ExternalId from voice), so matching is attempted before creating.

### 2.3 OTP Provisioning (IdentityOtpProvisioningService)

- **MPI usage:** ❌ No.  
- Flow: find user by phone (Email), create user if missing; find patient by `ApplicationUserId`, then by `PhoneNumber`; if none, create new `Patient` and link to user.  
- Does **not** call `IMasterPatientIndexService.FindMatchAsync`.  
- **Gap:** Same phone could already have a patient from another flow; OTP path does not reuse that patient via MPI and may create a duplicate.

---

## 3. External ID Mapping

### 3.1 When PatientExternalId Is Created

- **CreatePatientHandler** (only path that creates `PatientExternalId`):  
  When `request.ClinicId != Guid.Empty`, it adds a `PatientExternalId` with:
  - `SourceSystem = request.ClinicId.ToString()`
  - `ExternalId = request.ExternalId?.Trim() ?? patient.Id.ToString()`

So **SourceSystem** and **ExternalId** are populated correctly when creating from a clinic context.

### 3.2 SourceSystem / ExternalId Population

| Flow | SourceSystem | ExternalId | Notes |
|------|--------------|------------|--------|
| REST create with ClinicId | `ClinicId.ToString()` | From request or `patient.Id.ToString()` | ✅ Correct. |
| Voice onboarding | `request.ClinicId.ToString()` (via command) | Not set on command → fallback `patient.Id.ToString()` | ✅ Populated; not a clinic-assigned id. |
| OTP provisioning | N/A | N/A | No `PatientExternalId` created (no clinic/source in this flow). |

### 3.3 Missing External ID Mapping

- **OTP path:** Does not create any `PatientExternalId`. Acceptable if OTP is only for phone-based login with no clinic/source; if later you need “patient from external system” for OTP-created patients, this would need to be added.
- **Voice flow:** Does not pass a clinic-specific external id (e.g. clinic chart number); `ExternalId` is the new patient’s Id. No bug, but no mapping to an external system’s id when that id is not provided.

---

## 4. Unique Constraint Safety

### 4.1 Database Constraints (Verified)

- **NationalHealthId:** Unique index with filter `[NationalHealthId] IS NOT NULL` (migration `20260316225353_MPI_UniqueIndexes`).
- **(SourceSystem, ExternalId):** Unique index on `PatientExternalIds` (same migration).

### 4.2 Application Handling of Constraint Violations

- **CreatePatientHandler:** No `try/catch` around `SaveChangesAsync`. A duplicate `NationalHealthId` or duplicate `(SourceSystem, ExternalId)` would surface as an unhandled **DbUpdateException** (or wrapped exception).
- **IdentityOtpProvisioningService:** Same; no handling of unique constraint violations.
- **CreatePatientFromVoiceHandler:** Delegates to CreatePatientHandler; no extra handling.

**Gap:** No handler catches `DbUpdateException` to detect duplicate key and resolve by loading the existing patient (e.g. by NationalHealthId or (SourceSystem, ExternalId)) and returning that patient’s Id. Concurrency or race conditions can lead to raw DB exceptions instead of a graceful “return existing patient” behavior.

---

## 5. Repository Queries for MPI

### 5.1 Find by NationalHealthId / Find by (SourceSystem, ExternalId)

- **IRepository&lt;Patient&gt;** (Application layer) exposes only: `GetByIdAsync`, `ListAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync`.  
  There are **no** repository methods such as `GetByNationalHealthIdAsync` or `GetBySourceSystemAndExternalIdAsync`.

- **MasterPatientIndexService** (Persistence) uses **ClinicalDbContext** directly and implements:
  - Find by **NationalHealthId** (indexed; unique when not null).
  - Find by **(SourceSystem, ExternalId)** via `PatientExternalIds` (unique index).
  - Find by **PhoneNumber** and by **demographics** (FirstName, LastName, DateOfBirth).

So the **logic and indexing** for “find by NationalHealthId” and “find by (SourceSystem, ExternalId)” exist and are used only inside the MPI service. Queries use the indexed columns; there are no separate repository APIs for these lookups.

---

## 6. Deliverable Summary

### 6.1 All Patient Creation Flows

| Flow | Entry Point | Creates Patient? | MPI Used? |
|------|-------------|------------------|-----------|
| REST create patient | `CreatePatientHandler` | Yes (when no match) | ✅ Yes |
| Voice onboarding | `CreatePatientFromVoiceHandler` → `CreatePatientCommand` → `CreatePatientHandler` | Yes (when no match) | ✅ Yes |
| OTP verify (phone login) | `VerifyOtpHandler` → `IdentityOtpProvisioningService.EnsureUserAndPatientForPhoneAsync` | Yes (when no patient by user id or phone) | ❌ No |

### 6.2 Unsafe Creation Paths

- **IdentityOtpProvisioningService.EnsureUserAndPatientForPhoneAsync** – creates a patient without calling MPI; can create duplicates for the same phone if a patient was created via REST/voice and not yet linked to an application user.

### 6.3 Missing External ID Mapping

- OTP path does not create `PatientExternalId` (no clinic/source in current design).
- Voice path does not pass a clinic-specific `ExternalId` (uses `patient.Id` as fallback); acceptable when external id is not available.

### 6.4 Missing Exception Handling for Unique Constraints

- No handler catches **DbUpdateException** (or unique constraint violations) for:
  - Duplicate **NationalHealthId**
  - Duplicate **(SourceSystem, ExternalId)** on `PatientExternalIds`
- Recommendation: In CreatePatientHandler (and any other path that creates patients / external ids), catch constraint violations and resolve by loading the existing patient and returning its Id instead of failing.

---

## 7. Recommendations

1. **OTP provisioning:** Use `IMasterPatientIndexService.FindMatchAsync` in `IdentityOtpProvisioningService` before creating a new patient (e.g. pass `nationalHealthId: null`, `sourceSystem: null`, `externalId: null`, and at least `phoneNumber` and placeholder first/last name and DOB). If MPI returns a match, link that patient to the application user and return it instead of creating a new one.
2. **Unique constraint handling:** In `CreatePatientHandler` (and optionally in OTP after it uses MPI), wrap `SaveChangesAsync` in try/catch for `DbUpdateException`; on unique constraint violation, query the existing patient by NationalHealthId or (SourceSystem, ExternalId), and return that patient’s Id.
3. **Optional repository API:** If you want application-layer reuse, add `GetByNationalHealthIdAsync` and/or `FindBySourceSystemAndExternalIdAsync` to a patient lookup service or repository; ensure they use the same indexed columns as the MPI. The MPI can continue to own the full matching policy.

---

*Audit performed against the RaphCare codebase; implementation details may change with future commits.*
