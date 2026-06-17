using RaphCare.Domain.Common;

namespace RaphCare.Domain.Patients;

/// <summary>
/// Chronic condition diagnosed for a patient.
/// </summary>
public class ChronicCondition : BaseEntity
{
    public Guid PatientId { get; set; }
    public string ConditionName { get; set; } = string.Empty;
    public DateTime DiagnosedAt { get; set; }
    public string? Status { get; set; }

    public Patient Patient { get; set; } = null!;
}
