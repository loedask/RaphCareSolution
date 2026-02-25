using RaphCare.Domain.Common;

namespace RaphCare.Domain.Patients;

/// <summary>
/// General medical history entry for a patient.
/// </summary>
public class MedicalHistory : BaseEntity
{
    public Guid PatientId { get; set; }
    public string ConditionName { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime RecordedAt { get; set; }

    public Patient Patient { get; set; } = null!;
}
