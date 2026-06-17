using RaphCare.Domain.Common;

namespace RaphCare.Domain.Patients;

/// <summary>
/// Identified risk factor for a patient (e.g. for AI, analytics, screening).
/// </summary>
public class RiskFactor : BaseEntity
{
    public Guid PatientId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? Severity { get; set; }
    public DateTime IdentifiedAt { get; set; }

    public Patient Patient { get; set; } = null!;
}
