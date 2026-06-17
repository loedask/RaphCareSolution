using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.PatientNotifications.Commands.CreatePatientInAppNotification;

public sealed class CreatePatientInAppNotificationHandler(
    IRepository<PatientInAppNotification> notifications,
    IRepository<Patient> patients,
    IUnitOfWork unitOfWork,
    IPatientPushNotificationSender pushSender) : IRequestHandler<CreatePatientInAppNotificationCommand, Guid>
{
    private readonly IRepository<PatientInAppNotification> _notifications = notifications;
    private readonly IRepository<Patient> _patients = patients;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IPatientPushNotificationSender _pushSender = pushSender;

    public async Task<Guid> Handle(CreatePatientInAppNotificationCommand request, CancellationToken cancellationToken)
    {
        var patient = await _patients.GetByIdAsync(request.PatientId, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException(nameof(Patient), request.PatientId);

        var entity = new PatientInAppNotification
        {
            PatientId = patient.Id,
            Title = request.Title.Trim(),
            Body = request.Body.Trim(),
            Type = request.Type.Trim(),
            IsRead = false,
        };

        await _notifications.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        if (request.SendPush)
        {
            await _pushSender.SendToPatientAsync(
                patient.Id,
                entity.Title,
                entity.Body,
                entity.Type,
                cancellationToken).ConfigureAwait(false);
        }

        return entity.Id;
    }
}
