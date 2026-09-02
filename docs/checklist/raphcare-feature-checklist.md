# RaphCare feature checklist

PDF companion: [`raphcare-feature-checklist.pdf`](raphcare-feature-checklist.pdf) (regenerate with `scripts/Export-RaphCareFeatureChecklistPdf.ps1` whenever this file changes).

Plain-language twin for non-technical partners: [`raphcare-feature-checklist-partner.md`](raphcare-feature-checklist-partner.md) (and its PDF). Keep both in sync when status changes.

Track progress across **API**, **Web admin panel**, and **Mobile** (patient app). Design / visual parity for Mobile stays in [`../Mobile_Concept_Port.md`](../Mobile_Concept_Port.md); release gates stay in [`Mobile_Release_Ready_Checklist.md`](Mobile_Release_Ready_Checklist.md).

**Overall completion (this checklist):** **75%**  
**Without wearable Phase 2+ metrics** (activity / sleep / stress / temp / ECG / glucose rows): **79%**

**How to mark items and score %**

- `[x]` done = 100% of that item
- `[x] ... (partial)` = 50%
- `[ ] ... (partial)` = 25% (started, mostly open)
- `[ ]` not done = 0%
- `(blocked: ...)` still scored (usually 0% or 25% if also partial)
- `(out of scope)` and open `(optional)` / `(optional / later)` items are **excluded** from section and overall %
- Vertical table rows (API / Client / Mobile UI) count as one item = average of the three cells
- Section % = average of that section's scored items
- Overall % = average of all scored items in Steps 1-5 plus Cross-cutting

Each step ends with a short status note and its section %. Recalculate when you change checkboxes.

**Layer order for new HTTP contracts**

Backend (API + Application + Persistence) -> `RaphCare.Client` -> Web / Mobile. Do not duplicate API contracts inside Mobile.

Last reviewed: 2026-09-02

---

## Step 1: Admin clinic / hospital ops (outpatient) - 100%

Work under `api/admin/clinics` -> `IAdminClinicService` -> Blazor `Pages/Admin/Hospitals/*`.

### API + Client

- [x] Clinic list / detail / update
- [x] Register clinic + platform reference code + claim by reference or registration number + ensure membership
- [x] Patients: list, detail, grant / revoke access
- [x] Staff: invite, job (staff / doctor / pharmacist / lab technician), administrator flag, resend, cancel pending, remove
- [x] Facilities CRUD (physical + virtual)
- [x] Dashboard metrics
- [x] Clinical team (providers): list, detail, create, set active, create and delete schedules
- [x] Appointments: list, book, cancel, reschedule
- [x] Visits: start, get, complete, record vitals, SOAP, notes, prescriptions, lab orders (admin or doctor, InProgress)
- [x] Collection board: search pending prescriptions/labs, camera QR scan, call a code onto the waiting screen, dispense, complete lab result, cancel, undo, recent history, print slip or wall poster with QR, public waiting-screen token (clinic staff; visit may be closed)
- [x] Staff patient chart (read-only): medical info, emergency contacts, insurance, invoices, mood, care plans, diagnoses, prescriptions, SOAP / notes, labs
- [x] Clinic devices list
- [x] Tele-session start (Agora join info for admin)

### Admin panel (Web)

- [x] Hospitals index / register / claim
- [x] Hospital detail (overview, facilities, patients, clinical team, staff). Opening a hospital hides the platform sidebar; All hospitals, language, and sign out sit in the top bar. Detail page uses a command-deck identity header and a numbered section rail.
- [x] Shared admin UI (`AdminPageCard`, `AdminPageHeader`, `AdminStatCard`, `AdminReviewItem`, `AdminAlert`, `AdminEmptyState`, `AdminDefinitionItem`, `HospitalDeck*` command-deck pieces) used across hospital admin pages
- [x] Patient detail page (staff chart on hospital-deck: overview, clinical, coverage, devices, care)
- [x] Clinician (provider) detail and schedules (hospital-deck)
- [x] Appointments page (hospital-deck)
- [x] Visit page + vitals + visit-scoped clinical docs (add SOAP / notes / prescriptions / order labs on InProgress; hospital-deck)
- [x] Collection page (search by code / name / health ID; scan QR; call next; mark collected; enter lab result; cancel; undo; print slip or wall poster; open waiting screen). Command-deck header and numbered section rail. Waiting screen at `/display/{token}` shows pickup codes only.
- [x] Tele join page (hospital-deck)
- [x] Admin dashboard
- [x] Web UI language switcher (en / fr / ln / sw)
- [ ] Phone-usable admin (responsive layout, then thin install) `(optional / later)`: same web portal on phones; not a clinician MAUI app; not a full PWA. See [`../15_Web_Admin_On_Phone.md`](../15_Web_Admin_On_Phone.md)

### Mobile

- [ ] Admin / clinician hospital ops `(out of scope)`: patient app only; admin stays on Web

### Step 1 status

**100%.** Outpatient hospital admin is end-to-end on API + Client + Web, including a staff-only patient chart (view), visit documentation while a visit is in progress (hospital administrator or doctor), a collection board where pharmacy, lab, or general staff can complete prescriptions and lab results, and a waiting screen that shows pickup codes only. Clinician Mobile stays out of scope. Phone use of the same web admin is later (optional); see [`../15_Web_Admin_On_Phone.md`](../15_Web_Admin_On_Phone.md).

---

## Step 2: Inpatient (wards / beds / admissions), MVP - 100%

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

- [x] `/admin/hospitals/{id}/inpatient`: occupancy stats, command-deck header and numbered section rail, add capacity, admit, active list + discharge, beds-by-ward map with top-down bed tiles
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

**100%.** Lifecycle complete on API, Client, and Web (capacity, maintenance, transfer, history, discharge notes, patient search). Demo seed and companion docs done. Do not start clinician Mobile for this until product asks for it.

---

## Step 3: Patient Mobile (concept parity + APIs) - 62%

Concept: `C:\laragon\www\raphcare-mobile-app-concept`. Screens and tokens: `docs/Mobile_Concept_Port.md`. Flags: `RaphCare.Mobile.Kernel` / `FeatureFlags`.

### Shell, auth, onboarding

- [x] Landing / welcome
- [x] Sign-in (email) + verify email
- [x] Forgot password (email code + new password)
- [x] Register options: email / phone / voice
- [x] Phone OTP send / verify -> API JWT
- [x] Voice onboarding (record -> API)
- [x] Account created
- [x] Home dashboard + quick links
- [x] Monochrome tintable icons on Home, Profile, Privacy, and Help (no multicolor emoji)
- [x] Shell navigation and list loads hardened so failed taps / off-thread UI updates do not close the Android app
- [x] Feature-flagged navigation / under-construction fallback
- [x] Patient clinic selection: name search + `RC-…` reference code (Profile My clinic; registration picker); `X-Clinic-Id` from selected clinic with config fallback for demos; friendly tenant error messages

### Verticals (API + Client + MAUI)

| Area | API | Client | Mobile UI | Notes |
|------|-----|--------|-----------|-------|
| Appointments | [x] | [x] | [x] | List, book (clinic from My clinic and clinician name picker), detail |
| Care / telehealth | [x] | [x] | [x] | Request call + join; Agora on Android |
| Health records | [x] | [x] | [x] | List + detail + pickup codes and QR; scan wall poster to check in; Call notice + on-screen status |
| Devices / BLE vitals | [x] | [x] | [x] | Offline outbox retries |
| Insurance | [x] | [x] | [x] | Hub + add / detail |
| Billing | [x] | [x] | [x] | Hub + add payment method |
| Family members | [x] | [x] | [x] | List / add / detail |
| Mental health | [x] | [x] | [x] | Content + mood check-in |
| AI assistant | [x] | [x] | [x] | Real Azure OpenAI or placeholder reply |
| Notifications | [x] | [x] | [x] | List / mark read / push registration |
| Settings / profile | [x] | [x] | [x] | Edit, personal info, medical info, emergency contacts, privacy, help, language, change password, My clinic (name search / RC- code) |

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
- [ ] End-to-end video call on a real Android phone `(manual)`: server token OK; needs camera device. See `docs/10_Agora_Twilio_Setup.md` §2.7
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
- [x] Vitals sync to API + offline outbox retry (`FileVitalsSyncOutbox`): HR / SpO₂ batches only
- [x] SKU-agnostic wearable capability catalog documented (`docs/14_Wearable_Capability_Catalog.md`)
- [x] HBand Android vendor path (JNI): download script + `HBandAndroidWearableBridge` + connect/pwd/person + live HR/SpO₂ start `(partial)`: needs physical ET580/ET585 verification; iOS not wired
- [ ] Reliable full band data (HBand-class parity for in-scope metrics) `(partial)`: Phase 1 HR/SpO₂ hooks in; activity/sleep/history still open. See `docs/14`
- [ ] Full typed HBand SDK C# binding project `(out of scope for Phase 1)`: JNI bridge used instead; optional later. See `docs/12_HBand_SDK_Integration.md`
- [ ] Live HR + SpO₂ in patient app via vendor protocol verified on hardware `(partial)`: wired; awaiting device test
- [ ] Auto sync / background monitoring of band readings `(partial)`: manual sync + outbox exist; background unproven; OS limits in `docs/14`
- [ ] Activity (steps / kcal / distance / goals) domain + sync + mobile `(not started)`
- [ ] Sleep domain + sync + mobile `(not started)`
- [ ] Stress domain + sync + mobile `(not started)`
- [ ] Body temperature domain + sync + mobile `(not started)`
- [ ] ECG / PPG capture + sync `(not started)`: domain `ECGReading` exists; no patient sync / BLE yet
- [ ] Glucose / body composition / Health Glance from band `(not started)`: gate clinical use; see guardrails in `docs/14`
- [ ] Weather / companion pushes to watch `(out of scope for clinical v1)` unless product expands lifestyle parity
- [ ] Y6 Pro (4G SOS / fall / emergency fleet workflow) end-to-end in app `(partial / product path)`: webhook + SMS + clinic admin UI (patient chart + hospital Devices board); OEM mapping / push alerts / full app vertical still open
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
- [x] Store packaging: icons, splash, package ids, privacy strings for Play / App Store `(partial)`: Android package id `com.yindula.raphcare`; Azure + Play Internal runbook; **`RaphCare.Web.Host`** for Linux App Service (Staging `ApiBaseUrl` on app **raphcare**); Play Console upload and iOS store still manual

### Admin panel

- [ ] Patient-app verticals on Web `(out of scope)`: patients use Mobile; staff use admin hospital flows

### Step 3 status

**62%.** Concept screens and patient APIs are largely in. Patients can pick a clinic by name or reference code. Home and Profile use monochrome tintable icons. Shell navigation and list loads are hardened against Android tap crashes. Android is ahead of iOS for video; BLE E580/E585 path exists; wearable catalog is in `docs/14`. Catch-up: phone/OS verification, iOS Agora, push config, store packaging, and wearables depth (live HR/SpO₂, auto sync, activity/sleep and related metrics).

---

## Step 4: Staff / shared clinical APIs (non-admin-clinic) - 83%

Broader API surface used by staff tools or integrations (not the patient app primary path).

- [x] `PatientsController`, `ClinicalController`, `AppointmentsController` (staff-shaped)
- [x] `DevicesController`, `TelemedicineController`, `BillingController`, `InsuranceController`
- [x] `MentalHealthController` (staff assessments: placeholder data until persistence)
- [x] `AIController`, `ReportingController`
- [x] FHIR export (`api/fhir`)
- [x] Standalone emergency webhook
- [x] Clinic admin surfaces emergency events (patient chart + hospital Devices board)
- [ ] Staff mental-health assessments persistence `(partial)`: list exists; real store TBD
- [ ] Reporting depth / real dashboards beyond placeholders `(partial)` as product defines

### Step 4 status

**83%.** Core staff and integration controllers are in. Mental-health persistence and reporting depth still thin.

---

## Step 5: Hospital plan value (stay, counter, money) - 75%

Work that makes the Hospital plan more than beds plus a waiting screen. Clinic stays outpatient. Network keeps multi-site index and group reporting. YC healthcare companies we adapted from: [Kaigo Health](https://www.ycombinator.com/companies/kaigo-health) (post-discharge follow-up), [Locata](https://www.ycombinator.com/companies/locata) (referral completion), [Adentris](https://www.ycombinator.com/companies/adentris) (discharge summaries), [YouShift](https://www.ycombinator.com/companies/youshift) (who is on today). We do not copy US denial-appeals products.

### API + Application + Persistence

- [x] Domain: `InpatientObservation`, admission `DischargeSummary` / `InvoiceId`, invoice `AdmissionId` / `PaymentMethod`, `InvoiceLineItem` mapped in billing
- [x] `GET` / `POST {clinicId}/admissions/{id}/observations` (nurse, doctor, administrator, or general staff)
- [x] Discharge: patient-facing summary, bed-night invoice, optional extra line, mark paid in cash
- [x] Nurse job role (seed, map, staff invite)
- [x] Occupancy %, admissions and discharges today, average stay on inpatient board and hospital dashboard
- [x] Patient health records list and detail include discharged stays (`RecordKind` Discharge)
- [x] Lab results on the visit health record `(partial)`: already on visit detail; no separate "result ready" notice
- [x] Casualty / triage queue (waiting-screen style, codes only): tickets, call, complete, public `/display/casualty/{token}`
- [x] Theatre list (today's cases): schedule, start, complete, cancel on staff board
- [x] Emergency events on the hospital dashboard home (not only Devices): overview + `/admin` when a hospital is selected (SOS / fall, last 72h)
- [x] Outbound referral tracking so a sent referral is followed to completion
- [ ] Book a return visit at discharge
- [ ] Who is on today (simple roster)
- [ ] AI draft of the discharge summary (Hospital include; Clinic keeps the paid add-on)

Shipped on API, Client, and Web admin. Discharged stays also show in the patient Health records list. Casualty, theatre, and referral boards are on the hospital rail. SafeCare SOS and fall alerts also sit on the hospital overview and admin home.

### Step 5 status

**75%.** Ward notes, discharge invoice, occupancy, nurse role, discharge summaries, casualty queue, theatre list, emergency alerts on the hospital home, and outbound referral tracking are in. Return booking, roster, and AI draft are not started.

---

## Cross-cutting - 88%

- [x] Solution layers: Domain -> Application -> Persistence / Infrastructure / Identity -> API; Client -> Web / Mobile
- [x] `RaphCare.Client` + `AddRaphCareClient` for Web and Mobile
- [x] Dual auth: Entra JWT (staff) + patient OTP / email JWT
- [x] MPI / patient merge pipeline (see audit docs)
- [x] Inpatient documented in companion docs (Step 2 docs rows)
- [x] Web UI localization (en / fr / ln / sw): `AppResources` + language picker on admin layout
- [x] Staging demo pack: email/password accounts on `RaphCare Demo Clinic` (idempotent; does not wipe other hospitals)
- [ ] E2E smoke script covering admin inpatient + one patient mobile vertical against a running API (plan: [`Web_And_Mobile_Smoke_Plan.md`](Web_And_Mobile_Smoke_Plan.md))

### Cross-cutting status

**88%.** Platform wiring includes Web UI languages and a Staging demo pack. Missing an automated E2E smoke against a running API. Plan for Web and Mobile smoke is documented; automation not started.

---

## Rollup

| Area | % | Notes |
|------|--:|-------|
| Step 1 Admin outpatient | 100% | Clinician Mobile out of scope |
| Step 2 Inpatient MVP | 100% | Clinician Mobile out of scope |
| Step 3 Patient Mobile | 62% | iOS video, push, wearables depth |
| Step 4 Staff / shared APIs | 83% | Persistence / reporting polish |
| Step 5 Hospital plan value | 75% | Referral board in; return booking and roster open |
| Cross-cutting | 88% | Demo pack on Staging; E2E smoke plan in, script open |
| **Overall (scored items)** | **75%** | Out of scope / open optional excluded |
| **Without wearable Phase 2+ metrics** | **79%** | Excludes activity / sleep / stress / temp / ECG / glucose rows |

**Next:** iOS Agora + dual-OS smoke, push config, wearable live vitals prove-out, then Phase A API smoke from [`Web_And_Mobile_Smoke_Plan.md`](Web_And_Mobile_Smoke_Plan.md).

---

## Quick surface map

| Surface | Primary owner | Status snapshot |
|---------|---------------|-----------------|
| **API** admin clinics | `AdminClinicsController` | Outpatient complete; inpatient lifecycle plus ward notes and discharge invoice; casualty queue and display token; theatre board; outbound referral board; collection board; waiting-screen display token; staff jobs Doctor / Pharmacist / LabTechnician / Nurse |
| **API** patient | `api/patient/*`, auth, onboarding | Verticals wired; collection-orders + health-record pickup codes; config-dependent push / AI / iOS RTC |
| **Client** | `IAdminClinicService`, patient `I*Service` | Matches current APIs |
| **Web admin** | `Pages/Admin/Hospitals/*` | Hospital ops + inpatient lifecycle + ward notes + discharge invoice + occupancy numbers + casualty + theatre + referrals + emergency home + staff patient chart (view) + visit documentation on InProgress + collection counter + waiting screens; UI en/fr/ln/sw; phone use later (responsive + thin install, not a full PWA) |
| **Mobile** | `RaphCare.Mobile` patient app | Concept screens in; Android ahead of iOS for video; BLE partial |

---

## Related docs

| Doc | Use for |
|-----|---------|
| `raphcare-feature-checklist-partner.md` | Plain-language status for non-technical partners |
| [`Web_And_Mobile_Smoke_Plan.md`](Web_And_Mobile_Smoke_Plan.md) | Planned API, Web UI, and Mobile smoke layers |
| [`../Mobile_Concept_Port.md`](../Mobile_Concept_Port.md) | Tokens, route map, screen visual parity |
| `Mobile_Release_Ready_Checklist.md` | Flags, quality bar, release backlog |
| [`../09_Mobile_App_Guide.md`](../09_Mobile_App_Guide.md) | Mobile structure, DI, config |
| [`../11_Devices_BLE_E580_E585.md`](../11_Devices_BLE_E580_E585.md) | BLE bands (E580 / E585) |
| [`../12_HBand_SDK_Integration.md`](../12_HBand_SDK_Integration.md) | HBand SDK binding path |
| [`../13_Patient_Device_Packages_and_Fleet.md`](../13_Patient_Device_Packages_and_Fleet.md) | Y6 / E580 / E585 fleet SKUs |
| [`../14_Wearable_Capability_Catalog.md`](../14_Wearable_Capability_Catalog.md) | SKU-agnostic band features + delivery phases |
| [`../10_Agora_Twilio_Setup.md`](../10_Agora_Twilio_Setup.md) | Telehealth RTC setup |
| [`../15_Web_Admin_On_Phone.md`](../15_Web_Admin_On_Phone.md) | Staff phone use of web admin: responsive UI, then thin install; not a full PWA |
| [`../02_Solution_Structure.md`](../02_Solution_Structure.md) | Project layout |
| [`../06_Key_Workflows.md`](../06_Key_Workflows.md) | Workflow narratives (update when inpatient ships docs) |
