# RaphCare Dev Companion — Change Log

## Date

2026-03-15

## High-level summary of changes

Documentation was re-analyzed against the current codebase and updated so that the RaphCare Dev Companion knowledge files stay aligned with the solution. Updates reflect phone OTP authentication (send/verify), API-issued JWT for patients, voice onboarding (CreatePatientFromVoice), and related entities, services, and endpoints.

## New modules added

- **Auth (OTP):** Application feature and AuthController for phone-based OTP send/verify; IOtpService, ITokenService; API-issued JWT for verified patients.
- **Onboarding (Voice):** Application feature and VoiceOnboardingController for voice-first patient creation; ISpeechToTextService, CreatePatientFromVoiceCommand; VoiceRecording entity and ClinicalDbContext VoiceRecordings.

## Modules modified

- **RaphCare.API:** Added AuthController (api/auth/otp — POST send, POST verify), VoiceOnboardingController (api/onboarding — POST voice, multipart).
- **RaphCare.Application:** Added interfaces IOtpService, ITokenService, ISpeechToTextService; features Auth (SendOtp, VerifyOtp), Onboarding (CreatePatientFromVoice); TranscriptionResult DTO.
- **RaphCare.Domain:** Added OtpCode (Identity), VoiceRecording (Patients); Patient has VoiceRecordings collection.
- **RaphCare.Infrastructure:** Added OtpService, TokenService (JwtOptions), AzureSpeechToTextService (placeholder); OtpCodeConfiguration, VoiceRecordingConfiguration.
- **RaphCare.Persistence:** IdentityDbContext — OtpCodes DbSet and OtpCodeConfiguration; ClinicalDbContext — VoiceRecordings DbSet and VoiceRecordingConfiguration; IRepository&lt;VoiceRecording&gt; registration.

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
