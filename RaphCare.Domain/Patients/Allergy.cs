using RaphCare.Domain.Common;

namespace RaphCare.Domain.Patients;

/// <summary>
/// Recorded allergy for a patient.
/// </summary>
public class Allergy : BaseEntity
{
    public Guid PatientId { get; set; }
    public string Substance { get; set; } = string.Empty;
    public string? Reaction { get; set; }
    public string? Severity { get; set; }
    public DateTime RecordedAt { get; set; }

    public Patient Patient { get; set; } = null!;
}
