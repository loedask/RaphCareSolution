using RaphCare.Client.Contracts;
using RaphCare.Client.Models.Notifications;

namespace RaphCare.Client.Contracts.Interfaces;

/// <summary>Patient notification center and push registration.</summary>
public interface IPatientNotificationsService
{
    Task<Response<IReadOnlyList<PatientNotificationViewModel>>> GetMyNotificationsAsync(CancellationToken cancellationToken = default);

    Task<Response<bool>> MarkReadAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Response<bool>> MarkAllReadAsync(CancellationToken cancellationToken = default);

    Task<Response<bool>> RegisterPushDeviceAsync(RegisterPatientPushDeviceRequest request, CancellationToken cancellationToken = default);
}
