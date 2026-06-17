using RaphCare.Domain.Common;

namespace RaphCare.Domain.Clinical;

/// <summary>
/// History of status changes for a visit.
/// </summary>
public class VisitStatusHistory : BaseEntity
{
    public Guid VisitId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
    public string? ChangedBy { get; set; }

    public Visit Visit { get; set; } = null!;
}
