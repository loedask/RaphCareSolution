namespace RaphCare.API.Controllers;

public sealed class CreateAdminClinicAppointmentRequest
{
    public Guid PatientId { get; set; }
    public Guid ProviderId { get; set; }
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public string Type { get; set; } = "InPerson";
    public string? Reason { get; set; }
}
