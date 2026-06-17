namespace RaphCare.Client.Models.Appointments;

public class BookAppointmentRequest
{
    public Guid ClinicId { get; set; }
    public Guid ProviderId { get; set; }
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public string Type { get; set; } = "InPerson";
    public string Reason { get; set; } = string.Empty;
}
