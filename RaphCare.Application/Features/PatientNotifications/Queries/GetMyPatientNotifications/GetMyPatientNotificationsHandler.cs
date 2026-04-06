using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.PatientNotifications.DTOs;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.PatientNotifications.Queries.GetMyPatientNotifications;

public sealed class GetMyPatientNotificationsHandler(
    IRepository<PatientInAppNotification> notifications,
    ICurrentUserService currentUser) : IRequestHandler<GetMyPatientNotificationsQuery, IReadOnlyList<PatientNotificationDto>>
{
    private readonly IRepository<PatientInAppNotification> _notifications = notifications;
    private readonly ICurrentUserService _currentUser = currentUser;

    public async Task<IReadOnlyList<PatientNotificationDto>> Handle(GetMyPatientNotificationsQuery request, CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required.");

        var page = await _notifications.SearchAsync(
            q => q.Where(n => n.PatientId == patientId).OrderByDescending(n => n.CreatedAt),
            1,
            200,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        return page.Items.Select(PatientNotificationMappings.ToDto).ToList();
    }
}
