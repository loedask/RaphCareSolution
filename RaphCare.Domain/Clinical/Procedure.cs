using RaphCare.Domain.Common;

namespace RaphCare.Domain.Clinical;

/// <summary>
/// Procedure performed during a visit.
/// </summary>
public class Procedure : BaseEntity
{
    public Guid VisitId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime PerformedAt { get; set; }

    public Visit Visit { get; set; } = null!;
}
