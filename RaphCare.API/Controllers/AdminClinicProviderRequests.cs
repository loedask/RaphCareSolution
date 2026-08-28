namespace RaphCare.API.Controllers;

public sealed class CreateAdminClinicProviderRequest
{
    public Guid UserId { get; set; }
    public string? LicenseNumber { get; set; }
}

public sealed class CreateAdminClinicProviderScheduleRequest
{
    public DayOfWeek Day { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
}

public sealed class SetAdminClinicProviderActiveRequest
{
    public bool IsActive { get; set; }
}
