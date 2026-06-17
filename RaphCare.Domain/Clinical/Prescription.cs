using RaphCare.Domain.Common;

namespace RaphCare.Domain.Clinical;

/// <summary>
/// Prescription issued during a visit.
/// </summary>
public class Prescription : BaseEntity
{
    public Guid VisitId { get; set; }
    public DateTime IssuedAt { get; set; }
    public string? Notes { get; set; }

    public Visit Visit { get; set; } = null!;
    public ICollection<PrescriptionItem> PrescriptionItems { get; set; } = new List<PrescriptionItem>();
}
