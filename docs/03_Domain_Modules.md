# Domain Modules

## Identity

- **Purpose:** Users, roles, permissions, sessions, audit, and phone OTP codes for authentication and authorization.
- **Entities:** ApplicationUser, Role, Permission, UserRole, RolePermission, RefreshTokenRecord, UserSession, AuditLog, LoginAudit, AccessPolicy, OtpCode.
- **Persistence:** IdentityDbContext (Users, Roles, Permissions, UserRoles, RolePermissions, OtpCodes).
- **Services:** EntraTokenValidator, EntraUserProvisioningService, EntraRoleMapper (RaphCare.Identity); IApplicationUserStore (implemented in RaphCare.Persistence.ApplicationUserStore); IOtpService (OtpService in Infrastructure, backed by IdentityDbContext).
- **Controllers:** AuthController at `api/auth/otp` — POST send (SendOtpCommand), POST verify (VerifyOtpCommand); [AllowAnonymous]. Staff auth remains middleware and policy-based.
- **Relationships:** Identity is referenced by all other modules for current user and tenant; ApplicationUser is provisioned from Entra or from OTP verify (phone stored in Email).

---

## Organization

- **Purpose:** Clinics, facilities, departments, providers, schedules.
- **Entities:** Clinic, Department, Facility, Provider, Administrator, Therapist, ServiceOffering, ProviderSchedule, AvailabilityBlock, SupportStaff.
- **Persistence:** Clinic (and related) in ClinicalDbContext (Clinics DbSet).
- **Relationships:** Clinic is the tenant scope; patients, appointments, and visits are tied to clinic.

---

## Patients

- **Purpose:** Patient registration and demographics.
- **Entities:** Patient, PatientProfile, InsuranceProfile, Address, EmergencyContact, Allergy, Medication, MedicalHistory, ChronicCondition, FamilyHistory, ConsentRecord, VoiceRecording, etc.
- **Application:** CreatePatient, UpdatePatient, GetPatientById, GetPatients (paginated).
- **Controllers:** PatientsController — GET by id (Name = "GetPatientById"), GET list (Name = "GetPatientsPaginated"; pageNumber, pageSize), POST (Name = "CreatePatient"), PUT by id (Name = "UpdatePatient"). ProducesResponseType for typed Swagger/NSwag.
- **Persistence:** ClinicalDbContext (Patients, VoiceRecordings).
- **Relationships:** Patient belongs to Clinic; can have InsuranceProfile and VoiceRecordings; linked to Appointments, Visits, DeviceAssignments.

---

## Clinical (Appointments & Visits)

- **Purpose:** Appointments, clinical visits, care plans, prescriptions, notes, lab results.
- **Entities:** Appointment, Visit, CarePlan, Prescription, PrescriptionItem, ClinicalNote, LabResult, Diagnosis, Procedure, SOAPNote, VitalSignRecord, Referral, etc.
- **Application:** Create/Update/Get Appointment; Create/Update/Get Visit; GetVisits (paginated, optional clinicId).
- **Controllers:** AppointmentsController (CRUD); ClinicalController (visits: GET by id, GET list, POST, PUT).
- **Persistence:** ClinicalDbContext (Appointments, Visits, CarePlans, VoiceRecordings, etc.).
- **Relationships:** Appointment and Visit reference Patient and Clinic; Visit can have prescriptions, notes, lab results; VoiceRecording references Patient.

---

## Devices

- **Purpose:** Device registry, types, manufacturers, firmware, assignments, and readings (heart rate, blood pressure, glucose, etc.).
- **Entities:** Device, DeviceType, DeviceManufacturer, DeviceFirmware, DeviceAssignment, DeviceReading, HeartRateReading, BloodPressureReading, GlucoseReading, etc.
- **Application:** CreateDevice, UpdateDevice, GetDeviceById, GetDevices (paginated).
- **Controllers:** DevicesController — GET by id, GET list, POST, PUT.
- **Persistence:** DeviceDbContext (Devices, DeviceTypes, DeviceManufacturers, DeviceFirmwares, DeviceAssignments).
- **Relationships:** Device can be assigned to a patient; readings are domain entities (storage may be in same or separate context).

---

## Insurance

- **Purpose:** Insurance plans and patient insurance profiles.
- **Entities:** InsurancePlan, InsuranceProfile (domain), Subscription, Claim, CoverageRule, PlanBenefit, EligibilityRule, etc.
- **Application:** Create/Update InsuranceProfile, GetInsuranceProfileById, GetInsuranceProfiles (paginated).
- **Controllers:** InsuranceController — profiles: GET by id, GET list, POST, PUT.
- **Persistence:** InsuranceDbContext (InsurancePlans, InsuranceProfiles).
- **Relationships:** InsuranceProfile links Patient to InsurancePlan; seeding creates sample InsurancePlan.

---

## Billing

- **Purpose:** Invoices and billing-related data.
- **Entities:** Invoice, InvoiceLineItem, PaymentTransaction, BillingProfile, etc.
- **Application:** CreateInvoice, UpdateInvoice, GetInvoiceById, GetInvoices (paginated).
- **Controllers:** BillingController — GET by id, GET list, POST, PUT.
- **Persistence:** BillingDbContext (Invoices).
- **Relationships:** Invoices typically reference Patient/Clinic; no payment gateway integration in repo.

---

## Telemedicine

- **Purpose:** Tele-sessions and related questionnaire/summary data.
- **Entities:** TeleSession, TeleSessionParticipant, TeleSessionRecording, PreVisitQuestionnaire, QuestionnaireResponse, PostVisitSummary, etc.
- **Application:** CreateTeleSession, GetTeleSessionById.
- **Controllers:** TelemedicineController — GET session by id, POST sessions.
- **Persistence:** ClinicalDbContext (TeleSessions).
- **Relationships:** TeleSession linked to Patient/Visit/Clinic.

---

## Communication

- **Purpose:** Messages and conversations (in-app).
- **Entities:** Message, Conversation, MessageAttachment, Notification, EmailLog, SMSLog, etc.
- **Application:** CreateMessage, UpdateMessage, GetMessageById, GetMessages.
- **Persistence:** ClinicalDbContext (Messages).
- **Relationships:** Messages can be scoped to conversation/patient/clinic.

---

## Mental Health

- **Purpose:** Assessments and therapy-related data; patient hub mood check-ins.
- **Entities:** MentalHealthAssessment, AssessmentQuestion, AssessmentResponse, TherapySession, TherapyNote, MoodLog, WellnessCheckIn, etc.
- **Application:** GetMentalHealthAssessments (clinicId, pagination); patient hub **GetMyPatientMentalHealthContent**, **LogMyPatientMoodCheckIn** (config-driven copy + `MoodLog` in Clinical).
- **Controllers:** `MentalHealthController` — GET `api/MentalHealth/assessments` (staff). **`PatientMentalHealthController`** — GET `api/patient/mental-health/content`, POST `api/patient/mental-health/mood-checkin` (patient JWT).
- **Persistence:** `MoodLogs` on **ClinicalDbContext**; other mental health entities TBD.
- **Relationships:** Assessments tied to clinic/patient; mood logs tied to patient (cascade delete).

---

## AI

- **Purpose:** AI-generated summaries and insights; risk and wellness data for dashboards.
- **Entities (domain):** WellnessInsight, RiskScore, TrendAnalysis, AIRecommendation, etc.
- **Application:** IAIService (GenerateSummary); GenerateSummary command/handler; placeholder implementation in Infrastructure.
- **Controllers:** AIController — POST api/ai/summary (GenerateSummary).
- **Persistence:** AIDbContext (WellnessInsights, RiskScores, DashboardSnapshots).
- **Relationships:** Consumes clinical/visit context; no external AI SDK in repo (placeholder only).

---

## Reporting

- **Purpose:** Dashboard and operational reporting.
- **Entities:** DashboardSnapshot, ClinicPerformance, ProviderPerformance, OperationalReport, etc.
- **Application:** GetDashboardSnapshot (query clinicId, snapshotDate).
- **Controllers:** ReportingController — GET dashboard (clinicId, snapshotDate).
- **Persistence:** AIDbContext (DashboardSnapshots).
- **Relationships:** Aggregates data from clinical and other contexts.

---

## Auth (OTP)

- **Purpose:** Phone-based OTP send/verify for patient onboarding and low-friction access; API-issued JWT for verified patients.
- **Application:** SendOtpCommand (rate-limited; IOtpService + ISmsService), VerifyOtpCommand (IOtpService validate, provision ApplicationUser, assign Patient role, LoginAudit, ITokenService.GeneratePatientToken).
- **Controllers:** AuthController (see Identity). Endpoints: POST api/auth/otp/send, POST api/auth/otp/verify; returns token on success.
- **Services:** IOtpService (generate/validate, hash stored in IdentityDbContext.OtpCodes), ITokenService (API-issued JWT via JwtOptions: Issuer, Audience, Secret).
- **Relationships:** Uses Identity (OtpCode, ApplicationUser, UserRole, LoginAudit).

---

## Onboarding (Voice)

- **Purpose:** Voice-first patient onboarding: accept audio, transcribe, extract fields, create patient, store recording reference.
- **Application:** CreatePatientFromVoiceCommand — ISpeechToTextService.TranscribeAsync, CreatePatientCommand, update patient phone, create VoiceRecording; CreatePatientFromVoiceResult (PatientId, Transcription).
- **Controllers:** VoiceOnboardingController at `api/onboarding` — POST voice (multipart: audioFile, language, phoneNumber, clinicId); [AllowAnonymous].
- **Services:** ISpeechToTextService (TranscriptionResult: FullText, ExtractedFields); AzureSpeechToTextService placeholder in Infrastructure.
- **Persistence:** ClinicalDbContext (Patients, VoiceRecordings); IRepository&lt;VoiceRecording&gt; and IRepository&lt;Patient&gt;.
- **Relationships:** Consumes Auth (OTP-verified phone assumed by client/session); creates Patient and VoiceRecording.
