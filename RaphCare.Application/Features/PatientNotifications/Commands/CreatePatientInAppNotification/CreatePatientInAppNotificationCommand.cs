using MediatR;

namespace RaphCare.Application.Features.PatientNotifications.Commands.CreatePatientInAppNotification;

/// <summary>Creates an in-app notification row; optionally requests push delivery through the registered <c>IPatientPushNotificationSender</c>.</summary>
public sealed class CreatePatientInAppNotificationCommand : IRequest<Guid>
{
    public Guid PatientId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool SendPush { get; set; }
}
