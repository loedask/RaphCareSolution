namespace RaphCare.Application.Features.Organization.DTOs;

public sealed class AdminClinicPatientDetailDto
{
    public Guid PatientId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string AccessType { get; set; } = string.Empty;
    public DateTime GrantedAt { get; set; }
    public string GrantedByRule { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public IReadOnlyList<AdminClinicPatientVisitDto> RecentVisits { get; set; } = Array.Empty<AdminClinicPatientVisitDto>();
    public IReadOnlyList<AdminClinicPatientAppointmentDto> Appointments { get; set; } = Array.Empty<AdminClinicPatientAppointmentDto>();
    public IReadOnlyList<AdminClinicPatientVitalDto> RecentVitals { get; set; } = Array.Empty<AdminClinicPatientVitalDto>();
    public IReadOnlyList<AdminClinicPatientDeviceReadingDto> RecentDeviceReadings { get; set; } = Array.Empty<AdminClinicPatientDeviceReadingDto>();
    public IReadOnlyList<AdminClinicPatientDeviceRollupDto> DeviceDailyRollups { get; set; } = Array.Empty<AdminClinicPatientDeviceRollupDto>();
}

public sealed class AdminClinicPatientAppointmentDto
{
    public Guid Id { get; set; }
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string ProviderName { get; set; } = string.Empty;
}

public sealed class AdminClinicPatientVisitDto
{
    public Guid Id { get; set; }
    public DateTime VisitStart { get; set; }
    public DateTime? VisitEnd { get; set; }
    public string VisitType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Summary { get; set; }
}

public sealed class AdminClinicPatientVitalDto
{
    public string Type { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string? Unit { get; set; }
    public DateTime RecordedAt { get; set; }
    public string? VisitType { get; set; }
}

public sealed class AdminClinicPatientDeviceReadingDto
{
    public string Kind { get; set; } = string.Empty;
    public string ReadingType { get; set; } = string.Empty;
    public decimal PrimaryValue { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime RecordedAt { get; set; }
    public int? HeartRateBpm { get; set; }
    public decimal? SpO2Percent { get; set; }
}

public sealed class AdminClinicPatientDeviceRollupDto
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
