namespace RaphCare.Client.Models;

public sealed class ClinicDashboard
{
    public Guid ClinicId { get; set; }
    public string ClinicName { get; set; } = string.Empty;
    public int PatientCount { get; set; }
    public int StaffCount { get; set; }
    public int ProviderCount { get; set; }
    public int FacilityCount { get; set; }
    public int AppointmentsTodayCount { get; set; }
    public int UpcomingAppointmentsCount { get; set; }
    public IReadOnlyList<ClinicAppointmentListItem> UpcomingAppointments { get; set; } = Array.Empty<ClinicAppointmentListItem>();
}

public sealed class ClinicProviderListItem
{
    public Guid ProviderId { get; set; }
    public Guid ApplicationUserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int ScheduleSlotCount { get; set; }
}

public sealed class ClinicProviderDetail
{
    public Guid ProviderId { get; set; }
    public Guid ApplicationUserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public IReadOnlyList<ClinicProviderSchedule> Schedules { get; set; } = Array.Empty<ClinicProviderSchedule>();
}

public sealed class ClinicProviderSchedule
{
    public Guid Id { get; set; }
    public DayOfWeek Day { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsRecurring { get; set; }
}

public sealed class ClinicAppointmentListItem
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public Guid ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public Guid? ActiveVisitId { get; set; }
}

public sealed class PagedClinicAppointments
{
    public IReadOnlyList<ClinicAppointmentListItem> Items { get; set; } = Array.Empty<ClinicAppointmentListItem>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public sealed class CreateProviderScheduleRequest
{
    public DayOfWeek Day { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
}

public sealed class BookClinicAppointmentRequest
{
    public Guid PatientId { get; set; }
    public Guid ProviderId { get; set; }
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public string Type { get; set; } = "InPerson";
    public string? Reason { get; set; }
}

public sealed class RescheduleClinicAppointmentRequest
{
    public Guid? ProviderId { get; set; }
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public string? Reason { get; set; }
}

public sealed class ClinicPatientAppointmentSummary
{
    public Guid Id { get; set; }
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string ProviderName { get; set; } = string.Empty;
}

public sealed class ClinicPatientVitalSummary
{
    public string Type { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string? Unit { get; set; }
    public DateTime RecordedAt { get; set; }
    public string? VisitType { get; set; }
}

public sealed class ClinicPatientDeviceReadingSummary
{
    public string Kind { get; set; } = string.Empty;
    public string ReadingType { get; set; } = string.Empty;
    public decimal PrimaryValue { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime RecordedAt { get; set; }
    public int? HeartRateBpm { get; set; }
    public decimal? SpO2Percent { get; set; }
}

public sealed class ClinicPatientDeviceRollupSummary
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

public sealed class ClinicVisitDetail
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid AppointmentId { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public Guid ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public DateTime VisitStart { get; set; }
    public DateTime? VisitEnd { get; set; }
    public string VisitType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public IReadOnlyList<ClinicVisitVital> Vitals { get; set; } = Array.Empty<ClinicVisitVital>();
}

public sealed class ClinicVisitVital
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string? Unit { get; set; }
    public DateTime RecordedAt { get; set; }
}

public sealed class RecordVisitVitalRequest
{
    public string Type { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string? Unit { get; set; }
    public DateTime? RecordedAt { get; set; }
}

public sealed class ClinicDeviceListItem
{
    public Guid Id { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsAssigned { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? AssignedPatientId { get; set; }
    public string? AssignedPatientName { get; set; }
}

public sealed class ClinicTeleJoinInfo
{
    public Guid TeleSessionId { get; set; }
    public Guid VisitId { get; set; }
    public Guid AppointmentId { get; set; }
    public string ChannelName { get; set; } = string.Empty;
    public uint Uid { get; set; }
    public string? AppId { get; set; }
    public string? RtcToken { get; set; }
    public long TokenExpiresAtUnix { get; set; }
    public bool RtcConfigured { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ScheduledStart { get; set; }
    public string? PatientName { get; set; }
    public string? ProviderDisplayName { get; set; }
}
