using RaphCare.Domain.Common;

namespace RaphCare.Domain.Reporting;

/// <summary>
/// Population-level insight or analytics for a clinic.
/// </summary>
public class PopulationInsight : BaseEntity
{
    public Guid ClinicId { get; set; }
    public string InsightType { get; set; } = string.Empty;
    public string DataJson { get; set; } = string.Empty;
}
