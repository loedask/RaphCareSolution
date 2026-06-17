using RaphCare.Domain.Common;

namespace RaphCare.Domain.Reporting;

/// <summary>
/// User adoption metrics for a clinic.
/// </summary>
public class AdoptionMetric : BaseEntity
{
    public Guid ClinicId { get; set; }
    public int NewUsers { get; set; }
    public int ActiveUsers { get; set; }
    public DateTime RecordedAt { get; set; }
}
