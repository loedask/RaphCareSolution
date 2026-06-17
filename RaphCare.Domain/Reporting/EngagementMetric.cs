using RaphCare.Domain.Common;

namespace RaphCare.Domain.Reporting;

/// <summary>
/// Engagement metrics (telemedicine, devices, mood logs) for a clinic.
/// </summary>
public class EngagementMetric : BaseEntity
{
    public Guid ClinicId { get; set; }
    public int TeleSessions { get; set; }
    public int DeviceReadings { get; set; }
    public int MoodLogs { get; set; }
}
