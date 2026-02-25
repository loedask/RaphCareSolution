using RaphCare.Domain.Common;

namespace RaphCare.Domain.Clinical;

/// <summary>
/// Diagnosis recorded during a visit (ICD-10 ready).
/// </summary>
public class Diagnosis : BaseEntity
{
    public Guid VisitId { get; set; }
    public string Code { get; set; } = string.Empty; // ICD-10
    public string? Description { get; set; }
    public string? Severity { get; set; }

    public Visit Visit { get; set; } = null!;
}
