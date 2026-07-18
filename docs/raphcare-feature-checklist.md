# RaphCare feature checklist

PDF companion: [`raphcare-feature-checklist.pdf`](raphcare-feature-checklist.pdf) (regenerate with `scripts/Export-RaphCareFeatureChecklistPdf.ps1` whenever this file changes).

Track progress across **API**, **Web admin panel**, and **Mobile** (patient app). Design / visual parity for Mobile stays in `docs/Mobile_Concept_Port.md`; release gates stay in `docs/Mobile_Release_Ready_Checklist.md`.

**How to mark items**

- `[x]` done
- `[ ]` not done
- `(partial)` in the note when something works but is thinner than the intended product
- `(blocked: ...)` when another layer must land first
- `(out of scope)` when intentionally not planned for that surface

**Layer order for new HTTP contracts**

Backend (API + Application + Persistence) -> `RaphCare.Client` -> Web / Mobile. Do not duplicate API contracts inside Mobile.

Last reviewed: 2026-07-18

---

## Step 1: Admin clinic / hospital ops (outpatient)

Work under `api/admin/clinics` -> `IAdminClinicService` -> Blazor `Pages/Admin/Hospitals/*`.

### API + Client

- [x] Clinic list / detail / update
- [x] Register clinic + claim by registration number + ensure membership
- [x] Patients: list, detail, grant / revoke access
- [x] Staff: invite, role, resend, cancel pending, remove
- [x] Facilities CRUD (physical + virtual)
- [x] Dashboard metrics
- [x] Providers: list, detail, create, set active, schedules create / delete
- [x] Appointments: list, book, cancel, reschedule
- [x] Visits: start, get, complete, record vitals
- [x] Clinic devices list
- [x] Tele-session start (Agora join info for admin)

### Admin panel (Web)

- [x] Hospitals index / register / claim
- [x] Hospital detail (overview, facilities, patients, providers, staff)
- [x] Patient detail page
- [x] Provider detail + schedules
- [x] Appointments page
- [x] Visit page + vitals
- [x] Tele join page
- [x] Admin dashboard

### Mobile

- [ ] Admin / clinician hospital ops `(out of scope)`: patient app only; admin stays on Web

### Step 1 status

**Complete.** Outpatient hospital admin is end-to-end on API + Client + Web.

---

## Step 2: Inpatient (wards / beds / admissions), MVP

New vertical: Domain `Ward` / `Room` / `Bed` / `InpatientAdmission`, migration `AddInpatientBedsAndAdmissions`, `AdminClinicsController` inpatient actions, Client methods, Web `Inpatient.razor`.

### API + Application + Persistence

- [x] Domain entities + EF configs + migration
- [x] `GET {clinicId}/inpatient/board`
- [x] `POST {clinicId}/wards` / `rooms` / `beds`
- [x] `POST {clinicId}/admissions` (admit; one active stay; bed Available -> Occupied)
- [x] `POST {clinicId}/admissions/{id}/discharge` (free bed)
- [x] `IAdminClinicInpatientQueryService` board aggregation
- [ ] Update / deactivate / delete ward, room, bed
- [ ] Set bed status to `Maintenance` (status exists on domain; unused in handlers)
- [ ] Transfer patient between beds
- [ ] Admission history / get-by-id
- [ ] Soft-delete or archive capacity
- [ ] Demo seed sample wards / beds `(optional)`

### Client

- [x] `GetInpatientBoardAsync`, `CreateWard/Room/BedAsync`, `AdmitPatientAsync`, `DischargeAdmissionAsync`
- [x] Models in `ClinicOperationalModels` (`ClinicInpatientBoard`, ward/room/bed/admission)
- [ ] Client methods for update / maintenance / transfer `(blocked: API)`

### Admin panel (Web)

- [x] `/admin/hospitals/{id}/inpatient`: occupancy stats, add capacity, admit, active list + discharge, beds-by-ward map
- [x] Nav link from hospital detail
- [ ] Edit / delete / deactivate capacity UI `(blocked: API)`
- [ ] Maintenance toggle UI `(blocked: API)`
- [ ] Bed transfer UI `(blocked: API)`
- [ ] Admission history / detail
- [ ] Discharge notes field in UI `(partial)`: API accepts notes; UI may not expose them yet
- [ ] Patient picker beyond first page (100) `(partial)`

### Mobile

- [ ] Inpatient / bed board `(out of scope)` unless a clinician mobile surface is planned later  
  Client already has the HTTP methods if needed.

### Docs

- [ ] `docs/03_Domain_Modules.md`: Ward / Room / Bed / InpatientAdmission
- [ ] `docs/05_Database_Design.md`: inpatient tables
- [ ] `docs/06_Key_Workflows.md`: admit / discharge
- [ ] `docs/00_Change_Log.md`: inpatient entry

### Step 2 status

**MVP done on API -> Client -> Web.** Remaining rows are capacity lifecycle, transfer, history, and docs. Do not start clinician Mobile for this until product asks for it.

---

## Step 3: Patient Mobile (concept parity + APIs)

Concept: `C:\laragon\www\raphcare-mobile-app-concept`. Screens and tokens: `docs/Mobile_Concept_Port.md`. Flags: `RaphCare.Mobile.Kernel` / `FeatureFlags`.

### Shell, auth, onboarding

- [x] Landing / welcome
- [x] Sign-in (email) + verify email
- [x] Register options: email / phone / voice
- [x] Phone OTP send / verify -> API JWT
- [x] Voice onboarding (record -> API)
- [x] Account created
- [x] Home dashboard + quick links
- [x] Feature-flagged navigation / under-construction fallback

### Verticals (API + Client + MAUI)

| Area | API | Client | Mobile UI | Notes |
|------|-----|--------|-----------|-------|
| Appointments | [x] | [x] | [x] | List / book / detail |
| Care / telehealth | [x] | [x] | [x] | Request call + join; Agora on Android |
| Health records | [x] | [x] | [x] | List + detail |
| Devices / BLE vitals | [x] | [x] | [x] | Offline outbox retries |
| Insurance | [x] | [x] | [x] | Hub + add / detail |
| Billing | [x] | [x] | [x] | Hub + add payment method |
| Family members | [x] | [x] | [x] | List / add / detail |
| Mental health | [x] | [x] | [x] | Content + mood check-in |
| AI assistant | [x] | [x] | [x] | Real Azure OpenAI or placeholder reply |
| Notifications | [x] | [x] | [x] | List / mark read / push registration |
| Settings / profile | [x] | [x] | [x] | Edit, personal info, medical info, emergency contacts, privacy, help, language, change password |

Screen visual parity table is marked done in `Mobile_Concept_Port.md` (last pass). Re-check when the React concept changes.

### Mobile cross-cutting left

- [ ] iOS Agora RTC wiring `(partial)`: Android Agora in; see `docs/10_Agora_Twilio_Setup.md`
- [ ] Push in production `(partial)`: FCM when Firebase service account configured; else no-op
- [ ] AI assistant production LLM `(partial)`: needs `PatientAssistant` Azure OpenAI config
- [ ] Dark mode resource dictionary `(out of scope for v1)` unless product reverses
- [ ] Deep links / NotFound route UX
- [ ] Prod feature-flag rollout plan filled in `Mobile_Release_Ready_Checklist.md`
- [ ] iOS + Android release verification per screen (use release checklist tables)

### Admin panel

- [ ] Patient-app verticals on Web `(out of scope)`: patients use Mobile; staff use admin hospital flows

### Step 3 status

**Concept screens and patient APIs are largely in.** Remaining work is platform/ops polish (iOS RTC, push/LLM config, flags, release verification), not greenfield features.

---

## Step 4: Staff / shared clinical APIs (non-admin-clinic)

Broader API surface used by staff tools or integrations (not the patient app primary path).

- [x] `PatientsController`, `ClinicalController`, `AppointmentsController` (staff-shaped)
- [x] `DevicesController`, `TelemedicineController`, `BillingController`, `InsuranceController`
- [x] `MentalHealthController` (staff assessments: placeholder data until persistence)
- [x] `AIController`, `ReportingController`
- [x] FHIR export (`api/fhir`)
- [x] Standalone emergency webhook
- [ ] Staff mental-health assessments persistence `(partial)`: list exists; real store TBD
- [ ] Reporting depth / real dashboards beyond placeholders `(partial)` as product defines

---

## Cross-cutting

- [x] Solution layers: Domain -> Application -> Persistence / Infrastructure / Identity -> API; Client -> Web / Mobile
- [x] `RaphCare.Client` + `AddRaphCareClient` for Web and Mobile
- [x] Dual auth: Entra JWT (staff) + patient OTP / email JWT
- [x] MPI / patient merge pipeline (see audit docs)
- [ ] Inpatient documented in companion docs (Step 2 docs rows)
- [ ] E2E smoke script covering admin inpatient + one patient mobile vertical against a running API

---

## Quick surface map

| Surface | Primary owner | Status snapshot |
|---------|---------------|-----------------|
| **API** admin clinics | `AdminClinicsController` | Outpatient complete; inpatient MVP complete |
| **API** patient | `api/patient/*`, auth, onboarding | Verticals wired; config-dependent push / AI / iOS RTC |
| **Client** | `IAdminClinicService`, patient `I*Service` | Matches current APIs |
| **Web admin** | `Pages/Admin/Hospitals/*` | Hospital ops + inpatient MVP |
| **Mobile** | `RaphCare.Mobile` patient app | Concept screens in; release / platform gaps |

---

## Related docs

| Doc | Use for |
|-----|---------|
| `Mobile_Concept_Port.md` | Tokens, route map, screen visual parity |
| `Mobile_Release_Ready_Checklist.md` | Flags, quality bar, release backlog |
| `09_Mobile_App_Guide.md` | Mobile structure, DI, config |
| `02_Solution_Structure.md` | Project layout |
| `06_Key_Workflows.md` | Workflow narratives (update when inpatient ships docs) |
