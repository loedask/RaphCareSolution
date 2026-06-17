using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.PatientNotifications.Commands.MarkMyPatientNotificationRead;

public sealed class MarkMyPatientNotificationReadHandler(
    IRepository<PatientInAppNotification> notifications,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser) : IRequestHandler<MarkMyPatientNotificationReadCommand, Unit>
{
    private readonly IRepository<PatientInAppNotification> _notifications = notifications;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICurrentUserService _currentUser = currentUser;

    public async Task<Unit> Handle(MarkMyPatientNotificationReadCommand request, CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required.");

        var entity = await _notifications.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (entity is null || entity.PatientId != patientId)
            throw new NotFoundException(nameof(PatientInAppNotification), request.Id);

        entity.MarkAsRead();
        await _notifications.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}
