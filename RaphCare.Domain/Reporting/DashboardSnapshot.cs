using RaphCare.Domain.Common;

namespace RaphCare.Domain.Reporting;

/// <summary>
/// Cached dashboard snapshot for a clinic at a point in time.
/// </summary>
public class DashboardSnapshot : BaseEntity
{
    public Guid ClinicId { get; set; }
    public DateTime SnapshotDate { get; set; }
    public string SnapshotJson { get; set; } = string.Empty;
}
