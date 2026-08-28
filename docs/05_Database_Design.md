# Database Design

## DbContext Structure

Six bounded-context DbContexts live in RaphCare.Persistence. Each can use a dedicated connection string or fall back to DefaultConnection.

| DbContext | Connection string key | DbSets |
|-----------|------------------------|--------|
| IdentityDbContext | IdentityConnection (or Default) | Users (ApplicationUser), Roles, Permissions, UserRoles, RolePermissions, OtpCodes |
| ClinicalDbContext | ClinicalConnection (or Default) | Patients, Clinics, Appointments, Visits, CarePlans, TeleSessions, Messages, VoiceRecordings, Wards, Rooms, Beds, InpatientAdmissions, MoodLogs, MentalHealthAssessments, PatientPushDevices, … |
| DeviceDbContext | DeviceConnection (or Default) | Devices, DeviceTypes, DeviceManufacturers, DeviceFirmwares, DeviceAssignments |
| InsuranceDbContext | InsuranceConnection (or Default) | InsurancePlans, InsuranceProfiles |
| BillingDbContext | BillingConnection (or Default) | Invoices |
| AIDbContext | (Default) | WellnessInsights, RiskScores, DashboardSnapshots |

All contexts use the same migrations assembly: `RaphCare.Persistence`. Interceptors registered: AuditableEntityInterceptor, SoftDeleteInterceptor, DomainEventDispatcherInterceptor. Query tracking is NoTracking by default for read-oriented usage.

## Key Tables (by Context)

- **Identity:** ApplicationUser (e.g. Id, EntraObjectId, Email, DisplayName), Role, Permission, UserRole, RolePermission, OtpCode (PhoneNumber, CodeHash, ExpiresAt, IsUsed, CreatedAt, UsedAt).
- **Clinical:** Patient (Id, ClinicId, etc.), Clinic, Appointment, Visit, CarePlan, TeleSession, Message, VoiceRecording (PatientId, StorageUrl, DurationSeconds, Language), Ward / Room / Bed (inpatient capacity), InpatientAdmission (patient bed stay), MoodLog, MentalHealthAssessment, PatientPushDevice.
- **Devices:** Device, DeviceType, DeviceManufacturer, DeviceFirmware, DeviceAssignment.
- **Insurance:** InsurancePlan, InsuranceProfile.
- **Billing:** Invoice.
- **AI:** WellnessInsight, RiskScore, DashboardSnapshot.

Entity configurations are applied via EF Core model builder (configurations in Infrastructure.Persistence.Configurations or Persistence). Domain entities follow BaseEntity/AggregateRoot where applicable; soft delete and audit fields are applied via interceptors.

## Relationships

- Cross-context references are by ID only (no EF navigation across DbContexts). E.g. Clinical entities reference ClinicId; DeviceAssignment references patient/device.
- Within a context: standard FK and navigations as defined in configurations (e.g. Patient → Clinic, Visit → Patient, Appointment → Patient).

## Migration Strategy

- Migrations are generated and stored in RaphCare.Persistence (assembly RaphCare.Persistence). Each DbContext has its own set of migrations.
- **ApplyMigrationsAsync** (DatabaseMigrationExtensions, in App/Extensions): runs in **Development** and **Staging**. Creates a scope and calls `Database.MigrateAsync()` on ClinicalDbContext, DeviceDbContext, InsuranceDbContext, BillingDbContext, AIDbContext, IdentityDbContext in that order, then seeds. Invoked from API Program.cs after Build, before Run. Production does not auto-migrate.
- Production: migrations are not auto-applied by this code; deploy via your own process (e.g. CI/CD or manual).
