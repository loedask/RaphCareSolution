namespace RaphCare.Persistence.Seed;

/// <summary>Stable identifiers for local demo data (mobile <c>Appointments</c> defaults, telehealth E2E).</summary>
public static class ClinicalSeedIds
{
    public static readonly Guid DemoClinicId = Guid.Parse("11111111-1111-1111-1111-111111111101");
    /// <summary>Solo GP Practice plan demo site for consult waiting and plan gating.</summary>
    public static readonly Guid DemoPracticeClinicId = Guid.Parse("11111111-1111-1111-1111-111111111121");
    /// <summary>RaphCare Direct programme for private Health Track / SafeCare packages (packaging self-claim).</summary>
    public static readonly Guid DirectClinicId = Guid.Parse("11111111-1111-1111-1111-111111111120");
    public static readonly Guid DemoProviderId = Guid.Parse("11111111-1111-1111-1111-111111111102");
    public static readonly Guid DemoProviderApplicationUserId = Guid.Parse("11111111-1111-1111-1111-111111111103");
    public static readonly Guid DemoPatientId = Guid.Parse("11111111-1111-1111-1111-111111111104");
    public static readonly Guid DemoMentalHealthAssessmentId = Guid.Parse("11111111-1111-1111-1111-111111111105");
    public static readonly Guid DemoFacilityId = Guid.Parse("11111111-1111-1111-1111-111111111106");
    public static readonly Guid DemoWardId = Guid.Parse("11111111-1111-1111-1111-111111111107");
    public static readonly Guid DemoRoomId = Guid.Parse("11111111-1111-1111-1111-111111111108");
    public static readonly Guid DemoBedAId = Guid.Parse("11111111-1111-1111-1111-111111111109");
    public static readonly Guid DemoBedBId = Guid.Parse("11111111-1111-1111-1111-11111111110a");
    public static readonly Guid DemoAdminUserId = Guid.Parse("11111111-1111-1111-1111-11111111110b");
    public static readonly Guid DemoPharmacistUserId = Guid.Parse("11111111-1111-1111-1111-11111111110c");
    public static readonly Guid DemoLabUserId = Guid.Parse("11111111-1111-1111-1111-11111111110d");
    public static readonly Guid DemoPatientUserId = Guid.Parse("11111111-1111-1111-1111-11111111110e");
    public static readonly Guid DemoOpsUserId = Guid.Parse("11111111-1111-1111-1111-111111111116");
    public static readonly Guid DemoAppointmentId = Guid.Parse("11111111-1111-1111-1111-11111111110f");
    public static readonly Guid DemoVisitId = Guid.Parse("11111111-1111-1111-1111-111111111110");
    public static readonly Guid DemoPrescriptionId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid DemoLabRequestId = Guid.Parse("11111111-1111-1111-1111-111111111112");
    public static readonly Guid DemoInpatientPatientId = Guid.Parse("11111111-1111-1111-1111-111111111113");
    public static readonly Guid DemoCalledPrescriptionId = Guid.Parse("11111111-1111-1111-1111-111111111114");
    public static readonly Guid DemoAdmissionId = Guid.Parse("11111111-1111-1111-1111-111111111115");
    public static readonly Guid DemoPracticeAppointmentId = Guid.Parse("11111111-1111-1111-1111-111111111122");
    public static readonly Guid DemoPracticeConsultTicketId = Guid.Parse("11111111-1111-1111-1111-111111111123");
    public static readonly Guid DemoPracticeProviderId = Guid.Parse("11111111-1111-1111-1111-111111111124");
}
