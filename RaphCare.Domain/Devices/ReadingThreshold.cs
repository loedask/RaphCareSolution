using RaphCare.Domain.Common;

namespace RaphCare.Domain.Devices;

/// <summary>
/// Defines alert thresholds for a device type and metric.
/// </summary>
public class ReadingThreshold : BaseEntity
{
    public Guid DeviceTypeId { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public decimal MinValue { get; set; }
    public decimal MaxValue { get; set; }
    public bool IsActive { get; set; }

    public DeviceType DeviceType { get; set; } = null!;
}
