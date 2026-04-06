using MediatR;

namespace RaphCare.Application.Features.PatientNotifications.Commands.MarkMyPatientNotificationRead;

public sealed class MarkMyPatientNotificationReadCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
}
