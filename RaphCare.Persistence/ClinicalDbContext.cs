using Microsoft.EntityFrameworkCore;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Communication;
using RaphCare.Domain.Organization;
using RaphCare.Domain.MentalHealth;
using RaphCare.Domain.Patients;
using RaphCare.Domain.Telemedicine;
using RaphCare.Infrastructure.Persistence.Configurations;

namespace RaphCare.Persistence;

/// <summary>
/// Bounded context: Clinical (patients, visits, appointments, tele-sessions, messages). No financial entities.
/// </summary>
public class ClinicalDbContext(DbContextOptions<ClinicalDbContext> options) : DbContext(options)
{
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<PatientExternalId> PatientExternalIds => Set<PatientExternalId>();
    public DbSet<Clinic> Clinics => Set<Clinic>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Visit> Visits => Set<Visit>();
    public DbSet<CarePlan> CarePlans => Set<CarePlan>();
    public DbSet<TeleSession> TeleSessions => Set<TeleSession>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<VoiceRecording> VoiceRecordings => Set<VoiceRecording>();
    public DbSet<PatientMergeHistory> PatientMergeHistory => Set<PatientMergeHistory>();
    public DbSet<PatientIdentityEvent> PatientIdentityEvents => Set<PatientIdentityEvent>();
    public DbSet<PatientClinicAccess> PatientClinicAccesses => Set<PatientClinicAccess>();
    public DbSet<PatientFamilyMember> PatientFamilyMembers => Set<PatientFamilyMember>();
    public DbSet<MoodLog> MoodLogs => Set<MoodLog>();
    public DbSet<PatientInAppNotification> PatientInAppNotifications => Set<PatientInAppNotification>();
    public DbSet<PatientPushDevice> PatientPushDevices => Set<PatientPushDevice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new PatientConfiguration());
        modelBuilder.ApplyConfiguration(new PatientExternalIdConfiguration());
        modelBuilder.ApplyConfiguration(new ClinicConfiguration());
        modelBuilder.ApplyConfiguration(new VisitConfiguration());
        modelBuilder.ApplyConfiguration(new TeleSessionConfiguration());
        modelBuilder.ApplyConfiguration(new MessageConfiguration());
        modelBuilder.ApplyConfiguration(new VoiceRecordingConfiguration());
        modelBuilder.ApplyConfiguration(new PatientMergeHistoryConfiguration());
        modelBuilder.ApplyConfiguration(new PatientIdentityEventConfiguration());
        modelBuilder.ApplyConfiguration(new PatientClinicAccessConfiguration());
        modelBuilder.ApplyConfiguration(new PatientFamilyMemberConfiguration());
        modelBuilder.ApplyConfiguration(new MoodLogConfiguration());
        modelBuilder.ApplyConfiguration(new PatientInAppNotificationConfiguration());
        modelBuilder.ApplyConfiguration(new PatientPushDeviceConfiguration());
        modelBuilder.ApplyConfiguration(new VitalSignRecordConfiguration());
        modelBuilder.ApplyConfiguration(new ServiceOfferingConfiguration());
        modelBuilder.ApplyPersistenceConventions();
    }
}
