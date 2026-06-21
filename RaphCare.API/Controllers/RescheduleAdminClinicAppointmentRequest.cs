namespace RaphCare.API.Controllers;

public sealed class RescheduleAdminClinicAppointmentRequest
{
    public Guid? ProviderId { get; set; }
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public string? Reason { get; set; }
}
