using RaphCare.Domain.Common;

namespace RaphCare.Domain.Reporting;

/// <summary>
/// Aggregated performance snapshot for a provider.
/// </summary>
public class ProviderPerformance : BaseEntity
{
    public Guid ProviderId { get; set; }
    public Guid ClinicId { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalVisits { get; set; }
    public decimal PatientSatisfactionScore { get; set; }
}
