# RaphCare Dev Companion - Change Log

## Date

2026-08-29

## High-level summary of changes

Admin hospital sub-pages (patient chart, appointments, visit, provider, tele join) use the same hospital-deck chrome as hospital detail, inpatient, and collection: masthead, section rail, and staged panels instead of one long stack of cards.

## Modules modified

- **RaphCare.Web:** `Pages/Admin/Hospitals/Patient.razor`, `Appointments.razor`, `Visit.razor`, `Provider.razor`, and `TeleJoin.razor` restyled; en/fr/ln/sw strings for deck sections.

---

## Date

2026-08-23

## High-level summary of changes

Documented the future goal that staff use the existing web admin on a phone. Direction: responsive admin UI first, then a thin home-screen install. A full PWA (offline writes, staff store app) is not the plan. No product code change.

## Modules modified

- **docs:** `15_Web_Admin_On_Phone.md`; pointers in system overview, solution structure, and both feature checklists.

---

## Date

2026-07-21

## High-level summary of changes

Inpatient lifecycle completed in admin (capacity edit/delete, maintenance, transfer, admission history). Staff mental-health assessments persisted; reporting dashboard snapshot seeded. Mobile push device registration wired for demos; admin topbar search enabled. Y6 emergency events on admin patient chart and hospital Devices board. Agora server token path verified (Crc32.NET fix and smoke script). Companion docs updated.

## Modules modified

- **RaphCare.API / Application / Client / Web:** Admin inpatient update/delete/status/transfer/history endpoints and UI; Y6 emergency events on admin patient chart and hospital Devices board (`GET api/clinical/emergency-events`).
- **RaphCare.Persistence:** MentalHealthAssessments migration; ClinicalSeeder inpatient capacity and demo patient/assessment; ReportingSeeder dashboard snapshot.
- **RaphCare.Infrastructure:** Explicit **Crc32.NET** reference for Agora token minting.
- **RaphCare.Mobile:** Push registration service; iOS Agora session scaffolding.
- **tools:** `verify-agora-rtc-config.ps1` Agora token smoke.

## Database changes

- **Clinical:** MentalHealthAssessments table; demo seed for Facility / Ward / Room / Beds when empty.
- **AI:** Demo DashboardSnapshots row for demo clinic.

---

## Date

2026-03-15

## High-level summary of changes

Documentation was re-analyzed against the current codebase and updated so that the RaphCare Dev Companion knowledge files stay aligned with the solution. Updates reflect phone OTP authentication (send/verify), API-issued JWT for patients, voice onboarding (CreatePatientFromVoice), and related entities, services, and endpoints.

## New modules added

- **Auth (OTP):** Application feature and AuthController for phone-based OTP send/verify; IOtpService, ITokenService; API-issued JWT for verified patients.
- **Onboarding (Voice):** Application feature and VoiceOnboardingController for voice-first patient creation; ISpeechToTextService, CreatePatientFromVoiceCommand; VoiceRecording entity and ClinicalDbContext VoiceRecordings.

## Modules modified

- **RaphCare.API:** Added AuthController (api/auth/otp: POST send, POST verify), VoiceOnboardingController (api/onboarding: POST voice, multipart).
- **RaphCare.Application:** Added interfaces IOtpService, ITokenService, ISpeechToTextService; features Auth (SendOtp, VerifyOtp), Onboarding (CreatePatientFromVoice); TranscriptionResult DTO.
- **RaphCare.Domain:** Added OtpCode (Identity), VoiceRecording (Patients); Patient has VoiceRecordings collection.
- **RaphCare.Infrastructure:** Added OtpService, TokenService (JwtOptions), AzureSpeechToTextService (placeholder); OtpCodeConfiguration, VoiceRecordingConfiguration.
- **RaphCare.Persistence:** IdentityDbContext: OtpCodes DbSet and OtpCodeConfiguration; ClinicalDbContext: VoiceRecordings DbSet and VoiceRecordingConfiguration; IRepository&lt;VoiceRecording&gt; registration.

## Database changes

- **Identity:** OtpCode entity and OtpCodes table (PhoneNumber, CodeHash, ExpiresAt, IsUsed, CreatedAt, UsedAt); configuration in Infrastructure.Persistence.Configurations.
- **Clinical:** VoiceRecording entity and VoiceRecordings table (PatientId, StorageUrl, DurationSeconds, Language); FK to Patient; configuration in Infrastructure.

## Auth changes

- **Dual auth:** Staff continue to use Entra JWT; patients may use phone OTP verify and receive API-issued JWT (ITokenService.GeneratePatientToken, JwtOptions: Issuer, Audience, Secret).
- **OTP flow:** POST send (rate-limited, ISmsService), POST verify (validate, provision/find ApplicationUser by phone in Email, assign Patient role, LoginAudit, return token). AuthController [AllowAnonymous].

## Workflow updates

- **OTP Auth Flow:** Documented send and verify endpoints and handler flow.
- **Voice Onboarding Flow:** Documented POST api/onboarding/voice (multipart), CreatePatientFromVoiceHandler (transcribe, create patient, set phone, create VoiceRecording, return PatientId and Transcription).

## External integrations added/removed

- **Added (application-level):** ISpeechToTextService (AzureSpeechToTextService placeholder for voice onboarding). ITokenService for API-issued JWT (no external IdP). IOtpService (internal IdentityDbContext storage).
- **Removed:** None.
