namespace RaphCare.Application.Features.Organization.DTOs;

public sealed class AdminClinicAppointmentListItemDto
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
    public bool ConsentSigned { get; set; }
    public DateTime? ConsentSignedAt { get; set; }
}
