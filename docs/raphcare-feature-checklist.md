# RaphCare feature checklist

PDF companion: [`raphcare-feature-checklist.pdf`](raphcare-feature-checklist.pdf) (regenerate with `scripts/Export-RaphCareFeatureChecklistPdf.ps1` whenever this file changes).

Plain-language twin for non-technical partners: [`raphcare-feature-checklist-partner.md`](raphcare-feature-checklist-partner.md) (and its PDF). Keep both in sync when status changes.

Track progress across **API**, **Web admin panel**, and **Mobile** (patient app). Design / visual parity for Mobile stays in `docs/Mobile_Concept_Port.md`; release gates stay in `docs/Mobile_Release_Ready_Checklist.md`.

**How to mark items**

- `[x]` done
- `[ ]` not done
- `(partial)` in the note when something works but is thinner than the intended product
- `(blocked: ...)` when another layer must land first
- `(out of scope)` when intentionally not planned for that surface

**Layer order for new HTTP contracts**

Backend (API + Application + Persistence) -> `RaphCare.Client` -> Web / Mobile. Do not duplicate API contracts inside Mobile.

Last reviewed: 2026-07-21 (HBand Android JNI bridge Phase 1)

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
- [x] Update / deactivate / delete ward, room, bed
- [x] Set bed status to `Maintenance` (status exists on domain; unused in handlers)
- [x] Transfer patient between beds
- [x] Admission history / get-by-id
- [x] Soft-delete or archive capacity
- [x] Demo seed sample wards / beds `(optional)`

### Client

- [x] `GetInpatientBoardAsync`, `CreateWard/Room/BedAsync`, `AdmitPatientAsync`, `DischargeAdmissionAsync`
- [x] Models in `ClinicOperationalModels` (`ClinicInpatientBoard`, ward/room/bed/admission)
- [x] Client methods for update / maintenance / transfer / admission history

### Admin panel (Web)

- [x] `/admin/hospitals/{id}/inpatient`: occupancy stats, add capacity, admit, active list + discharge, beds-by-ward map
- [x] Nav link from hospital detail
- [x] Edit / delete / deactivate capacity UI
- [x] Maintenance toggle UI
- [x] Bed transfer UI
- [x] Admission history / detail
- [x] Discharge notes field in UI
- [x] Patient picker beyond first page (100) `(search + page size 50)`

### Mobile

- [ ] Inpatient / bed board `(out of scope)` unless a clinician mobile surface is planned later  
  Client already has the HTTP methods if needed.

### Docs

- [x] `docs/03_Domain_Modules.md`: Ward / Room / Bed / InpatientAdmission
- [x] `docs/05_Database_Design.md`: inpatient tables
- [x] `docs/06_Key_Workflows.md`: admit / discharge
- [x] `docs/00_Change_Log.md`: inpatient entry

### Step 2 status

**Lifecycle complete on API → Client → Web** (update/deactivate/delete capacity, maintenance, transfer, history, discharge notes, patient search). Demo seed + companion docs done. Do not start clinician Mobile for this until product asks for it.

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

### Mobile device coverage (phones, OS, wearables)

Phones / OS targets:

- [x] Android app build and run path (primary day-to-day target)
- [ ] iOS app build and run path verified on a physical iPhone `(partial)`: project targets iOS; release spot-check still open
- [ ] Same major flows verified on **both** Android and iOS before calling a vertical release-ready (use `Mobile_Release_Ready_Checklist.md` per-screen table)
- [ ] Windows MAUI target `(out of scope for BLE / most device work)`: BLE not active on Windows in this solution
- [ ] Mac Catalyst as a BLE test host `(optional)`: supported for Bluetooth perms; not a patient ship target

Telehealth video on device:

- [x] Android in-call UI + Agora RTC wiring
- [x] API Agora token mint verified (`tools/verify-agora-rtc-config.ps1`)
- [ ] iOS AgoraRtcKit (xcframework) wiring `(blocked / partial)`: see `docs/10_Agora_Twilio_Setup.md`
- [ ] End-to-end video call on a real Android phone `(manual)`: server token OK; needs camera device — see `docs/10_Agora_Twilio_Setup.md` §2.7
- [ ] End-to-end video call on a real iPhone `(blocked: iOS Agora)`

Push notifications on device:

- [x] App can register a push device token with the API
- [ ] Android FCM delivery in a configured environment `(partial)`: needs Firebase service account on API
- [ ] iOS push delivery (APNs / FCM path) verified on a physical iPhone `(partial)`
- [ ] Push off / no-op when Firebase is not configured (expected today)

Wearables / patient hardware (see `docs/11_Devices_BLE_E580_E585.md`, `docs/13_Patient_Device_Packages_and_Fleet.md`, **`docs/14_Wearable_Capability_Catalog.md`**):

- [x] Devices screen: BLE scan, connect, disconnect
- [x] E580 / E585 name filter (`E585E580DeviceFilter`, includes `ET580` / `ET585`) + "show all BLE" fallback
- [x] Android Bluetooth / Nearby devices permission flow
- [x] iOS Bluetooth usage string (`Info.plist`)
- [x] Heart-rate GATT read when the band exposes standard HR (`0x2A37`)
- [x] SpO₂ GATT read when the band exposes standard PLX (`0x2A5F` / `0x2A60`)
- [x] Vitals sync to API + offline outbox retry (`FileVitalsSyncOutbox`) — HR / SpO₂ batches only
- [x] SKU-agnostic wearable capability catalog documented (`docs/14_Wearable_Capability_Catalog.md`)
- [x] HBand Android vendor path (JNI): download script + `HBandAndroidWearableBridge` + connect/pwd/person + live HR/SpO₂ start `(partial)`: needs physical ET580/ET585 verification; iOS not wired
- [ ] Reliable full band data (HBand-class parity for in-scope metrics) `(partial)`: Phase 1 HR/SpO₂ hooks in; activity/sleep/history still open — see `docs/14`
- [ ] Full typed HBand SDK C# binding project `(out of scope for Phase 1)`: JNI bridge used instead; optional later — see `docs/12_HBand_SDK_Integration.md`
- [ ] Live HR + SpO₂ in patient app via vendor protocol verified on hardware `(partial)`: wired; awaiting device test
- [ ] Auto sync / background monitoring of band readings `(partial)`: manual sync + outbox exist; background unproven; OS limits in `docs/14`
- [ ] Activity (steps / kcal / distance / goals) domain + sync + mobile `(not started)`
- [ ] Sleep domain + sync + mobile `(not started)`
- [ ] Stress domain + sync + mobile `(not started)`
- [ ] Body temperature domain + sync + mobile `(not started)`
- [ ] ECG / PPG capture + sync `(not started)`: domain `ECGReading` exists; no patient sync / BLE yet
- [ ] Glucose / body composition / Health Glance from band `(not started)`: gate clinical use; see guardrails in `docs/14`
- [ ] Weather / companion pushes to watch `(out of scope for clinical v1)` unless product expands lifestyle parity
- [ ] Y6 Pro (4G SOS / fall / emergency fleet workflow) end-to-end in app `(partial / product path)`: fleet doc exists; app vertical still thinner than E580/E585 BLE path
- [ ] Clinic-facing view of patient device readings beyond admin devices list `(partial)`

Permissions / hardware UX:

- [x] Bluetooth permission prompts documented for Android 12+ and iOS
- [ ] Camera / mic permission path for telehealth verified on both OS `(partial)`
- [ ] Airplane / no-network degraded behavior spot-checked on a real phone
- [ ] Low-battery / Bluetooth-off empty states feel clear on Devices

### Mobile cross-cutting left

- [ ] AI assistant production LLM `(partial)`: needs `PatientAssistant` Azure OpenAI config
- [ ] Dark mode resource dictionary `(out of scope for v1)` unless product reverses
- [ ] Deep links / NotFound route UX
- [ ] Prod feature-flag rollout plan filled in `Mobile_Release_Ready_Checklist.md`
- [ ] Accessibility spot-check on phone (labels, contrast, key flows)
- [ ] Store packaging: icons, splash, package ids, privacy strings for Play / App Store

### Admin panel

- [ ] Patient-app verticals on Web `(out of scope)`: patients use Mobile; staff use admin hospital flows

### Step 3 status

**Concept screens and patient APIs are largely in.** Device coverage is broader than UI parity: Android is ahead of iOS for video; BLE E580/E585 path exists; wearable **capability catalog** is in `docs/14`; Y6 and full HBand SDK still open. Remaining work is phone/OS verification, iOS Agora, push config, flags, store packaging, and wearables depth (live HR/SpO₂ via vendor protocol, auto sync / background, activity/sleep).

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
- [x] Inpatient documented in companion docs (Step 2 docs rows)
- [ ] E2E smoke script covering admin inpatient + one patient mobile vertical against a running API

---

## Quick surface map

| Surface | Primary owner | Status snapshot |
|---------|---------------|-----------------|
| **API** admin clinics | `AdminClinicsController` | Outpatient complete; inpatient lifecycle complete |
| **API** patient | `api/patient/*`, auth, onboarding | Verticals wired; config-dependent push / AI / iOS RTC |
| **Client** | `IAdminClinicService`, patient `I*Service` | Matches current APIs |
| **Web admin** | `Pages/Admin/Hospitals/*` | Hospital ops + inpatient lifecycle |
| **Mobile** | `RaphCare.Mobile` patient app | Concept screens in; Android ahead of iOS for video; BLE partial |

---

## Related docs

| Doc | Use for |
|-----|---------|
| `raphcare-feature-checklist-partner.md` | Plain-language status for non-technical partners |
| `Mobile_Concept_Port.md` | Tokens, route map, screen visual parity |
| `Mobile_Release_Ready_Checklist.md` | Flags, quality bar, release backlog |
| `09_Mobile_App_Guide.md` | Mobile structure, DI, config |
| `11_Devices_BLE_E580_E585.md` | BLE bands (E580 / E585) |
| `12_HBand_SDK_Integration.md` | HBand SDK binding path |
| `13_Patient_Device_Packages_and_Fleet.md` | Y6 / E580 / E585 fleet SKUs |
| `14_Wearable_Capability_Catalog.md` | SKU-agnostic band features + delivery phases |
| `10_Agora_Twilio_Setup.md` | Telehealth RTC setup |
| `02_Solution_Structure.md` | Project layout |
| `06_Key_Workflows.md` | Workflow narratives (update when inpatient ships docs) |
