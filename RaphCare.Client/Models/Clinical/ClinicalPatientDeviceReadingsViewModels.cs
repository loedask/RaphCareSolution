namespace RaphCare.Client.Models.Clinical;

/// <summary>Single wearable reading row for staff/clinical dashboards (<c>api/clinical/patients/.../device-readings</c>).</summary>
public sealed class PatientDeviceReadingListItemViewModel
{
    public Guid Id { get; set; }
    public Guid DeviceId { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public string ReadingType { get; set; } = string.Empty;
    public double PrimaryValue { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime RecordedAt { get; set; }
    public DateTime ReceivedAt { get; set; }
    public int? HeartRateBpm { get; set; }
    public decimal? SpO2Percent { get; set; }
    public int? PulseRateBpm { get; set; }
}

/// <summary>Paged vitals for a patient in the current clinic context.</summary>
public sealed class PagedPatientDeviceReadingsViewModel
{
    public IReadOnlyList<PatientDeviceReadingListItemViewModel> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

/// <summary>Daily HR / SpO₂ aggregates for chart rollups.</summary>
public sealed class DeviceReadingDailyRollupViewModel
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

/// <summary>Standalone 4G emergency row (<c>api/clinical/patients/.../emergency-events</c>).</summary>
public sealed class PatientDeviceEmergencyEventViewModel
{
    public Guid Id { get; set; }
    public Guid DeviceId { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateTime OccurredAtUtc { get; set; }
    public DateTime ReceivedAtUtc { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? HorizontalAccuracyMeters { get; set; }
    public string? ExternalCorrelationId { get; set; }
    public bool CaregiversNotified { get; set; }
    public DateTime? CaregiversNotifiedAtUtc { get; set; }
    public string? CaregiverNotificationSummary { get; set; }
}

public sealed class PagedPatientDeviceEmergencyEventsViewModel
{
    public IReadOnlyList<PatientDeviceEmergencyEventViewModel> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
