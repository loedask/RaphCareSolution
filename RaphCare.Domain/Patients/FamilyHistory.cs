using RaphCare.Domain.Common;

namespace RaphCare.Domain.Patients;

/// <summary>
/// Family medical history entry for a patient.
/// </summary>
public class FamilyHistory : BaseEntity
{
    public Guid PatientId { get; set; }
    public string Condition { get; set; } = string.Empty;
    public string? Relationship { get; set; }
    public string? Notes { get; set; }

    public Patient Patient { get; set; } = null!;
}
