using RaphCare.Domain.Common;

namespace RaphCare.Domain.Patients;

/// <summary>
/// Tag for patient segmentation, AI, and analytics.
/// </summary>
public class PatientTag : BaseEntity
{
    public Guid PatientId { get; set; }
    public string TagName { get; set; } = string.Empty;
    public string? Description { get; set; }

    public Patient Patient { get; set; } = null!;
}
