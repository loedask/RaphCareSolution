namespace RaphCare.Client.Models.Appointments;

public sealed class AppointmentConsentViewModel
{
    public bool NeedsConsent { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool AlreadySigned { get; set; }
    public DateTime? SignedAt { get; set; }
}
