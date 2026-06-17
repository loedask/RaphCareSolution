using RaphCare.Domain.Common;

namespace RaphCare.Domain.Reporting;

/// <summary>
/// Aggregated performance snapshot for a clinic.
/// </summary>
public class ClinicPerformance : BaseEntity
{
    public Guid ClinicId { get; set; }
    public decimal Revenue { get; set; }
    public int ActivePatients { get; set; }
    public int ActiveSubscriptions { get; set; }
}
