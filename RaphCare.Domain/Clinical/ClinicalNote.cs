using RaphCare.Domain.Common;

namespace RaphCare.Domain.Clinical;

/// <summary>
/// Free-text or categorized clinical note for a visit.
/// </summary>
public class ClinicalNote : BaseEntity
{
    public Guid VisitId { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string? Category { get; set; }

    public Visit Visit { get; set; } = null!;
}
