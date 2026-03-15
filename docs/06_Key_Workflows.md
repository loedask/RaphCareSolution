# Key Workflows

## Patient Flow (Client)

- **Client:** RaphCare.Client PatientService uses generated IClient methods: GetPatientByIdAsync, GetPatientsPaginatedAsync, CreatePatientAsync, UpdatePatientAsync (operation names from API controller `Name`). Returns Response&lt;T&gt;; catches generated ApiException on failure.

## Appointment Flow

- **Create:** Client POSTs to api/appointments with body; AppointmentsController sends CreateAppointmentCommand via MediatR. Handler uses repository to add Appointment (linked to Patient/Clinic). Returns created resource or id.
- **Read:** GET api/appointments (list with pagination) or GET api/appointments/{id}. Handlers: GetAppointmentsQuery, GetAppointmentByIdQuery; return DTOs.
- **Update:** PUT api/appointments/{id} with body; UpdateAppointmentCommand and handler update entity and save.
- **Conventions:** Same CQRS pattern across: command/query, validator, handler, repository. No business logic in controller.

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

## Voice Onboarding Flow

- **Submit:** POST api/onboarding/voice (multipart: audioFile, language, phoneNumber, clinicId). CreatePatientFromVoiceHandler: ISpeechToTextService.TranscribeAsync, CreatePatientCommand, set patient phone, create VoiceRecording; returns PatientId and Transcription. OTP-verified phone is assumed by client/session.

## Admin Flow

- **Auth:** Admin uses Entra; role "Administrator" maps to RequireAdmin policy. TenantResolutionMiddleware requires X-Clinic-Id for /api/*.
- **Seeding:** DatabaseSeeder runs IdentitySeeder (roles/permissions), ClinicalSeeder (demo clinic), InsuranceSeeder (sample plan), DeviceSeeder and BillingSeeder (placeholders). Invoked separately (e.g. at startup or via a one-off); not part of API request pipeline.
- **Reporting:** GET api/reporting/dashboard (clinicId, snapshotDate) returns dashboard data; handler uses GetDashboardSnapshotQuery; data from AIDbContext (DashboardSnapshots) and related aggregates.
- **Migrations:** In Development, ApplyMigrationsAsync runs at API startup and applies all six DbContext migrations.

## Mobile App Flow

- **Startup:** AppShell registers all routes via AppNavigator.RegisterAllRoutes(). Shell shows LandingPage (auth) or HomePage (main) based on navigation.
- **Auth:** User lands on LandingPage; can go to Register (RegisterOptions → RegisterEmail → VerifyEmail) or SignIn. After successful Entra sign-in, app navigates to Home. SecureStorageAccessTokenProvider stores token and supplies it to RaphCare.Client via IAccessTokenProvider; all API calls use Bearer token.
- **Feature navigation:** AppNavigator.GoToFeatureAsync(route, featureDisplayName) navigates to a feature page; if the feature is disabled (FeatureFlags), shows UnderConstructionPage instead. Routes: Home, Records, Appointments, Insurance, Settings (and auth routes).
