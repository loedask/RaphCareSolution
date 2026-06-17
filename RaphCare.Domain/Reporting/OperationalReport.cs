using RaphCare.Domain.Common;

namespace RaphCare.Domain.Reporting;

/// <summary>
/// Operational report for a clinic (e.g. daily, weekly, monthly).
/// </summary>
public class OperationalReport : BaseEntity
{
    public Guid ClinicId { get; set; }
    public string ReportType { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string ReportDataJson { get; set; } = string.Empty;
}
