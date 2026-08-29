using RaphCare.Domain.Common;

namespace RaphCare.Domain.Clinical;

/// <summary>
/// Lab test requested during a visit.
/// </summary>
public class LabRequest : BaseEntity
{
    public Guid VisitId { get; set; }
    public string TestName { get; set; } = string.Empty;
    public string? Priority { get; set; }
    public DateTime RequestedAt { get; set; }
    public string Status { get; set; } = "Pending";
    public string PickupCode { get; set; } = string.Empty;
    public DateTime? CalledAt { get; set; }

    public Visit Visit { get; set; } = null!;
    public ICollection<LabResult> LabResults { get; set; } = new List<LabResult>();
}
