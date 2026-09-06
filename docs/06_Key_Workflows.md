# Key Workflows

## Patient Flow (Client)

- **Client:** RaphCare.Client PatientService uses generated IClient methods: GetPatientByIdAsync, GetPatientsPaginatedAsync, CreatePatientAsync, UpdatePatientAsync (operation names from API controller `Name`). Returns Response&lt;T&gt;; catches generated ApiException on failure.

## Appointment Flow

- **Provider / admin:** `api/Appointments` — `[Authorize(Policy = "RequireProvider")]`. Create/read/update as before (MediatR, CQRS).
- **Patient (mobile JWT with `patientId` claim):** `api/patient/appointments` — list (paged, newest first), GET by id (only own rows), POST to book (`CreatePatientAppointmentCommand`; patient id from token, not body).
- **Client:** `IAppointmentService` calls `api/patient/appointments` via `HttpClient` until NSwag is regenerated to include these routes.
- **Conventions:** Same CQRS pattern; no business logic in controllers.

## Visit Flow

- **Create:** POST api/clinical/visits; CreateVisitCommand and handler; Visit stored in ClinicalDbContext with link to Patient/Clinic.
- **Read:** GET api/clinical/visits (list, optional clinicId, pagination) or GET api/clinical/visits/{id}. Handlers: GetVisitsQuery, GetVisitByIdQuery.
- **Update:** PUT api/clinical/visits/{id}; UpdateVisitCommand and handler.
- **Relationship:** Visit is the clinical encounter; can be associated with appointments, prescriptions, notes, and tele-sessions.

## Insurance Flow

- **Plans:** InsurancePlan is reference data; seeded by InsuranceSeeder (e.g. "Standard Medical Aid"). Stored in InsuranceDbContext.
- **Profiles:** Patient insurance profiles (api/insurance/profiles). Create/Update/Get by id, Get list (paginated). Commands and queries via MediatR; handlers use repository against InsuranceDbContext. Links patient to plan and plan-specific data.

## Device Monitoring Flow

- **Registry:** Devices and related reference data (DeviceType, DeviceManufacturer, DeviceFirmware, DeviceAssignment) in DeviceDbContext. API: CRUD on api/devices (create, update, get by id, get list). Handlers use device repository.
- **Readings:** Domain defines reading types (e.g. HeartRateReading, BloodPressureReading, GlucoseReading). Storage and ingestion paths (e.g. background ingestion, device SDK) are not implemented in the repo; no external device SDK or job runner present.
- **Seeding:** DeviceSeeder is a placeholder (no-op with log).

## OTP Auth Flow

- **Send:** POST api/auth/otp/send with phone number. SendOtpHandler: IOtpService.GenerateOtpAsync (rate limit per phone), ISmsService sends code; returns 204 or 429.
- **Verify:** POST api/auth/otp/verify with phone and code. VerifyOtpHandler: validate OTP, find or create ApplicationUser (Email = phone), assign Patient role, LoginAudit, ITokenService.GeneratePatientToken; returns 200 with token or 400.
- **Local development:** `SmsService` is a placeholder (no real SMS). When **`IHostEnvironment.IsDevelopment()`** is true, the API logs a **warning** with prefix **`[Development] SMS not sent. OTP for testing`** including the phone number and full message (which contains the six-digit code). Run the API with `dotnet run` and watch the console (or Visual Studio / Cursor output) to copy the OTP into the mobile **Verify phone** screen.

## Voice Onboarding Flow

- **Submit:** POST api/onboarding/voice (multipart: audioFile, language, phoneNumber, clinicId). CreatePatientFromVoiceHandler: ISpeechToTextService.TranscribeAsync, CreatePatientCommand, set patient phone, create VoiceRecording; returns PatientId and Transcription. OTP-verified phone is assumed by client/session.

## Admin Flow

- **Auth:** Admin uses Entra; role "Administrator" maps to RequireAdmin policy. TenantResolutionMiddleware requires X-Clinic-Id for /api/*.
- **Inpatient:** Admin opens `/admin/hospitals/{id}/inpatient`. Board shows occupancy, admissions today, and average stay. Nurses and doctors add ward notes on an active stay. Admins discharge with an optional patient-facing summary, nightly bed rate, extra charge, and cash paid. History via `GET …/admissions`. Discharged stays appear in the patient health records list.
- **Casualty:** Admin opens `/admin/hospitals/{id}/casualty`. Staff enqueue a walk-in (optional patient), call a queue code onto `/display/casualty/{token}`, then complete or cancel. The TV shows codes and triage colour only.
- **Theatre:** Admin opens `/admin/hospitals/{id}/theatre`. Staff schedule today's cases, then mark InProgress, Completed, or Cancelled.
- **Referrals:** Admin opens `/admin/hospitals/{id}/referrals`. Staff log an outbound referral (destination, optional specialty and reason), then mark Accepted, Completed, or Cancelled.
- **SafeCare home:** Recent SOS and fall events load on the hospital overview and `/admin` when a hospital is selected. Full history remains on Devices.
- **Seeding:** DatabaseSeeder runs IdentitySeeder (roles/permissions), ClinicalSeeder (demo clinic, telehealth provider, demo patient + MH assessment, inpatient facility/ward/room/beds), InsuranceSeeder (sample plan), DeviceSeeder and BillingSeeder (placeholders), ReportingSeeder (demo dashboard snapshot). Invoked separately (e.g. at startup or via a one-off); not part of API request pipeline.
- **Reporting:** GET api/reporting/dashboard (clinicId, snapshotDate) returns dashboard data; handler uses GetDashboardSnapshotQuery; data from AIDbContext (DashboardSnapshots) and related aggregates.
- **Migrations:** In Development and Staging, ApplyMigrationsAsync runs at API startup and applies all six DbContext migrations (then seed).

## Mobile App Flow

- **Startup:** AppShell registers all routes via AppNavigator.RegisterAllRoutes(). Shell shows LandingPage (auth) or HomePage (main) based on navigation.
- **Auth:** User lands on LandingPage; can go to Register (**RegisterOptions** offers **Email**, **Phone**, or **Voice**) or SignIn.
  - **Email:** RegisterEmail → VerifyEmail → Entra sign-in → Home (same as before).
  - **Phone:** RegisterPhone (E.164 + OTP send) → VerifyPhone → `api/auth/otp/verify` returns a **patient JWT**; mobile stores it via `IAuthService.StoreApiSessionAsync` (same secure storage as Entra for `IAccessTokenProvider`) → Home.
  - **Voice:** RegisterVoiceIntro → RegisterPhone with `ContinueWith=Voice` → VerifyPhone → VoiceSubmit (**in-app recording** via Plugin.Maui.Audio, then multipart upload) → `api/onboarding/voice`. Requires **`Onboarding:VoiceRegistrationClinicId`** in mobile configuration (Guid of a clinic in your environment, e.g. after ClinicalSeeder).
- **Client layer:** `IOtpAuthService` posts JSON to `api/auth/otp/*` and reads the verify response body (JWT). The generated NSwag `VerifyAsync` does not surface that body—use `IOtpAuthService` from **RaphCare.Client** for OTP on mobile. `IVoiceOnboardingService` wraps the generated `IClient.VoiceAsync` multipart call.
- **Backend readiness:** OTP and voice endpoints are implemented (`AuthController`, `VoiceOnboardingController`). For real SMS you must configure **`ISmsService`** / **`IOtpService`** in Infrastructure and Persistence as per your environment; voice upload depends on **`ISpeechToTextService`** and related handlers.
- **Feature navigation:** AppNavigator.GoToFeatureAsync(route, featureDisplayName) navigates to a feature page; if the feature is disabled (FeatureFlags), shows UnderConstructionPage instead. Routes: Home, Records, Appointments, Insurance, Settings (and auth routes).
