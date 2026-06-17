using RaphCare.Domain.Common;

namespace RaphCare.Domain.Patients;

/// <summary>
/// Extended demographic and metadata for a patient.
/// </summary>
public class PatientProfile : BaseEntity
{
    public Guid PatientId { get; set; }
    public string? MaritalStatus { get; set; }
    public string? Occupation { get; set; }
    public string? PreferredLanguage { get; set; }
    public string? BloodType { get; set; }

    public Patient Patient { get; set; } = null!;
}
