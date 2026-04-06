using MediatR;

namespace RaphCare.Application.Features.PatientNotifications.Commands.MarkAllMyPatientNotificationsRead;

public sealed class MarkAllMyPatientNotificationsReadCommand : IRequest<int>;
