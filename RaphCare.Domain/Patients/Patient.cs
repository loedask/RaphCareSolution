using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.Patients;

/// <summary>
/// Core longitudinal record owner. Aggregate root for patient-related consistency.
/// </summary>
public class Patient : AggregateRoot, ISoftDelete
{
    /// <summary>
    /// Canonical identity link to the authentication user.
    /// Nullable to support legacy/manual patients without an account yet.
    /// </summary>
    public Guid? ApplicationUserId { get; private set; }

    /// <summary>
    /// National or government-issued health identifier (e.g., national health number).
    /// Optional and only populated where such a system exists.
    /// </summary>
    public string? NationalHealthId { get; private set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string? NationalIdNumber { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public PatientProfile? PatientProfile { get; set; }
    public ICollection<Address> Addresses { get; set; } = new List<Address>();
    public ICollection<EmergencyContact> EmergencyContacts { get; set; } = new List<EmergencyContact>();
    public ICollection<ConsentRecord> ConsentRecords { get; set; } = new List<ConsentRecord>();
    public ICollection<InsuranceProfile> InsuranceProfiles { get; set; } = new List<InsuranceProfile>();
    public ICollection<MedicalHistory> MedicalHistories { get; set; } = new List<MedicalHistory>();
    public ICollection<Allergy> Allergies { get; set; } = new List<Allergy>();
    public ICollection<ChronicCondition> ChronicConditions { get; set; } = new List<ChronicCondition>();
    public ICollection<Immunization> Immunizations { get; set; } = new List<Immunization>();
    public ICollection<Medication> Medications { get; set; } = new List<Medication>();
    public ICollection<FamilyHistory> FamilyHistories { get; set; } = new List<FamilyHistory>();
    public LifestyleProfile? LifestyleProfile { get; set; }
    public ICollection<RiskFactor> RiskFactors { get; set; } = new List<RiskFactor>();
    public ICollection<PatientTag> PatientTags { get; set; } = new List<PatientTag>();
    public ICollection<VoiceRecording> VoiceRecordings { get; set; } = new List<VoiceRecording>();
    public ICollection<PatientExternalId> ExternalIds { get; private set; } = new List<PatientExternalId>();

    public void LinkToApplicationUser(Guid applicationUserId)
    {
        if (applicationUserId == Guid.Empty)
        {
            throw new ArgumentException("ApplicationUserId cannot be empty.", nameof(applicationUserId));
        }

        ApplicationUserId = applicationUserId;
    }

    public void SetNationalHealthId(string? nationalHealthId)
    {
        NationalHealthId = string.IsNullOrWhiteSpace(nationalHealthId) ? null : nationalHealthId;
    }
}
