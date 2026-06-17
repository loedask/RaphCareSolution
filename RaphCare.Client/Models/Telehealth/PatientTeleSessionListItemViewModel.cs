namespace RaphCare.Client.Models.Telehealth;

public class PatientTeleSessionListItemViewModel
{
    public Guid Id { get; set; }
    public DateTime ScheduledStart { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
}
