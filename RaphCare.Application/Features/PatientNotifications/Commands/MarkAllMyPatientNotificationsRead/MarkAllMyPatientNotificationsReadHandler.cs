using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Application.Features.PatientNotifications.Commands.MarkAllMyPatientNotificationsRead;

public sealed class MarkAllMyPatientNotificationsReadHandler(
    IPatientInAppNotificationBulkWriter bulkWriter,
    ICurrentUserService currentUser) : IRequestHandler<MarkAllMyPatientNotificationsReadCommand, int>
{
    private readonly IPatientInAppNotificationBulkWriter _bulkWriter = bulkWriter;
    private readonly ICurrentUserService _currentUser = currentUser;

    public async Task<int> Handle(MarkAllMyPatientNotificationsReadCommand request, CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required.");

        return await _bulkWriter.MarkAllReadForPatientAsync(patientId, cancellationToken).ConfigureAwait(false);
    }
}
