namespace RaphCare.Application.Features.Appointments.DTOs;

public sealed class PatientAppointmentConsentDto
{
    public bool NeedsConsent { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool AlreadySigned { get; set; }
    public DateTime? SignedAt { get; set; }
}
