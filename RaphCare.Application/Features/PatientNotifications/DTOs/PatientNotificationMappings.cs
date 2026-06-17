using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.PatientNotifications.DTOs;

internal static class PatientNotificationMappings
{
    public static PatientNotificationDto ToDto(PatientInAppNotification n) =>
        new()
        {
            Id = n.Id,
            Title = n.Title,
            Body = n.Body,
            Type = n.Type,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt,
        };
}
