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
    public DbSet<MentalHealthAssessment> MentalHealthAssessments => Set<MentalHealthAssessment>();
    public DbSet<AssessmentQuestion> AssessmentQuestions => Set<AssessmentQuestion>();
    public DbSet<AssessmentResponse> AssessmentResponses => Set<AssessmentResponse>();
    public DbSet<TherapySession> TherapySessions => Set<TherapySession>();
    public DbSet<TherapyNote> TherapyNotes => Set<TherapyNote>();
    public DbSet<CrisisFlag> CrisisFlags => Set<CrisisFlag>();
    public DbSet<BehavioralCarePlan> BehavioralCarePlans => Set<BehavioralCarePlan>();
    public DbSet<TherapyGoal> TherapyGoals => Set<TherapyGoal>();
    public DbSet<PatientInAppNotification> PatientInAppNotifications => Set<PatientInAppNotification>();
    public DbSet<PatientPushDevice> PatientPushDevices => Set<PatientPushDevice>();
    public DbSet<PatientSupportMessage> PatientSupportMessages => Set<PatientSupportMessage>();
    public DbSet<ClinicStaffMembership> ClinicStaffMemberships => Set<ClinicStaffMembership>();
    public DbSet<ClinicStaffInvitation> ClinicStaffInvitations => Set<ClinicStaffInvitation>();
    public DbSet<Ward> Wards => Set<Ward>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Bed> Beds => Set<Bed>();
    public DbSet<InpatientAdmission> InpatientAdmissions => Set<InpatientAdmission>();
    public DbSet<InpatientObservation> InpatientObservations => Set<InpatientObservation>();
    public DbSet<CasualtyTicket> CasualtyTickets => Set<CasualtyTicket>();
    public DbSet<TheatreCase> TheatreCases => Set<TheatreCase>();
    public DbSet<Referral> Referrals => Set<Referral>();
    public DbSet<ClinicRosterEntry> ClinicRosterEntries => Set<ClinicRosterEntry>();

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
        modelBuilder.ApplyConfiguration(new MentalHealthAssessmentConfiguration());
        modelBuilder.ApplyConfiguration(new AssessmentQuestionConfiguration());
        modelBuilder.ApplyConfiguration(new AssessmentResponseConfiguration());
        modelBuilder.ApplyConfiguration(new TherapySessionConfiguration());
        modelBuilder.ApplyConfiguration(new TherapyNoteConfiguration());
        modelBuilder.ApplyConfiguration(new CrisisFlagConfiguration());
        modelBuilder.ApplyConfiguration(new BehavioralCarePlanConfiguration());
        modelBuilder.ApplyConfiguration(new TherapyGoalConfiguration());
        modelBuilder.ApplyConfiguration(new PatientInAppNotificationConfiguration());
        modelBuilder.ApplyConfiguration(new PatientPushDeviceConfiguration());
        modelBuilder.ApplyConfiguration(new ClinicStaffMembershipConfiguration());
        modelBuilder.ApplyConfiguration(new ClinicStaffInvitationConfiguration());
        modelBuilder.ApplyConfiguration(new VitalSignRecordConfiguration());
        modelBuilder.ApplyConfiguration(new SOAPNoteConfiguration());
        modelBuilder.ApplyConfiguration(new ClinicalNoteConfiguration());
        modelBuilder.ApplyConfiguration(new PrescriptionConfiguration());
        modelBuilder.ApplyConfiguration(new LabRequestConfiguration());
        modelBuilder.ApplyConfiguration(new ServiceOfferingConfiguration());
        modelBuilder.ApplyConfiguration(new WardConfiguration());
        modelBuilder.ApplyConfiguration(new RoomConfiguration());
        modelBuilder.ApplyConfiguration(new BedConfiguration());
        modelBuilder.ApplyConfiguration(new InpatientAdmissionConfiguration());
        modelBuilder.ApplyConfiguration(new InpatientObservationConfiguration());
        modelBuilder.ApplyConfiguration(new CasualtyTicketConfiguration());
        modelBuilder.ApplyConfiguration(new TheatreCaseConfiguration());
        modelBuilder.ApplyConfiguration(new ReferralConfiguration());
        modelBuilder.ApplyConfiguration(new ClinicRosterEntryConfiguration());
        modelBuilder.ApplyPersistenceConventions();
    }
}
