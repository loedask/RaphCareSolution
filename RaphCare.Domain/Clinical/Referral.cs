using RaphCare.Domain.Common;

namespace RaphCare.Domain.Clinical;

/// <summary>
/// Referral to another provider or facility from a visit.
/// </summary>
public class Referral : BaseEntity
{
    public Guid VisitId { get; set; }
    public string ReferredTo { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime ReferredAt { get; set; }

    public Visit Visit { get; set; } = null!;
}
