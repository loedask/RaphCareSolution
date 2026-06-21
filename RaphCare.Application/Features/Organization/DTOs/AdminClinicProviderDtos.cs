namespace RaphCare.Application.Features.Organization.DTOs;

public sealed class AdminClinicProviderListItemDto
{
    public Guid ProviderId { get; set; }
    public Guid ApplicationUserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int ScheduleSlotCount { get; set; }
}

public sealed class AdminClinicProviderDetailDto
{
    public Guid ProviderId { get; set; }
    public Guid ApplicationUserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public IReadOnlyList<AdminClinicProviderScheduleDto> Schedules { get; set; } = Array.Empty<AdminClinicProviderScheduleDto>();
}

public sealed class AdminClinicProviderScheduleDto
{
    public Guid Id { get; set; }
    public DayOfWeek Day { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsRecurring { get; set; }
}
