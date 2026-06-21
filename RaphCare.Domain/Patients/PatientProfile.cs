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

    /// <summary>Patient-reported primary care provider (self-service mobile / profile).</summary>
    public string? PrimaryCareProviderName { get; set; }

    /// <summary>Free-text allergies as entered by the patient (not structured clinical coding).</summary>
    public string? SelfReportedAllergies { get; set; }

    /// <summary>Free-text chronic conditions as entered by the patient.</summary>
    public string? SelfReportedChronicConditions { get; set; }

    /// <summary>Free-text medications as entered by the patient.</summary>
    public string? SelfReportedMedications { get; set; }

    public Patient Patient { get; set; } = null!;
}
