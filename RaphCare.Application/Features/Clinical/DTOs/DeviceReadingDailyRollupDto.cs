namespace RaphCare.Application.Features.Clinical.DTOs;

/// <summary>Daily aggregates of wearable vitals for dashboard sparklines (UTC calendar day of <see cref="RaphCare.Domain.Devices.DeviceReading.RecordedAt"/>).</summary>
public sealed class DeviceReadingDailyRollupDto
{
    public DateOnly Date { get; set; }

    public int HeartRateSampleCount { get; set; }
    public decimal? AvgHeartRateBpm { get; set; }
    public int? MinHeartRateBpm { get; set; }
    public int? MaxHeartRateBpm { get; set; }

    public int SpO2SampleCount { get; set; }
    public decimal? AvgSpO2Percent { get; set; }
    public decimal? MinSpO2Percent { get; set; }
    public decimal? MaxSpO2Percent { get; set; }
}
