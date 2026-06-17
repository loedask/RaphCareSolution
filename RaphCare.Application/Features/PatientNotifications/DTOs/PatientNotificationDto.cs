namespace RaphCare.Application.Features.PatientNotifications.DTOs;

/// <summary>One row in the patient in-app notification center.</summary>
public sealed class PatientNotificationDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
