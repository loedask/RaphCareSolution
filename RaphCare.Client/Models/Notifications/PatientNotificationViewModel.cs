namespace RaphCare.Client.Models.Notifications;

/// <summary>Row in the patient notification center (<c>GET api/patient/notifications</c>).</summary>
public sealed class PatientNotificationViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
