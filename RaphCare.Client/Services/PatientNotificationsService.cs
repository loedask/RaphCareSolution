using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Notifications;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client.Services;

/// <summary>Wraps generated <see cref="IClient"/> patient notification operations and maps to feature view models.</summary>
public sealed class PatientNotificationsService(IClient client) : IPatientNotificationsService
{
    private readonly IClient _client = client;

    public async Task<Response<IReadOnlyList<PatientNotificationViewModel>>> GetMyNotificationsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var list = await _client.GetMyPatientNotificationsAsync(cancellationToken).ConfigureAwait(false);
            var mapped = (list ?? Array.Empty<PatientNotificationDto>()).Select(Map).ToList();
            return Response<IReadOnlyList<PatientNotificationViewModel>>.Success(mapped);
        }
        catch (global::RaphCare.Client.Services.Base.ApiException ex)
        {
            return Response<IReadOnlyList<PatientNotificationViewModel>>.Failure(ex.Message, ex.StatusCode);
        }
    }

    public async Task<Response<bool>> MarkReadAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _client.MarkMyPatientNotificationReadAsync(id, cancellationToken).ConfigureAwait(false);
            return Response<bool>.Success(true);
        }
        catch (global::RaphCare.Client.Services.Base.ApiException ex)
        {
            return Response<bool>.Failure(ex.Message, ex.StatusCode);
        }
    }

    public async Task<Response<bool>> MarkAllReadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _client.MarkAllMyPatientNotificationsReadAsync(cancellationToken).ConfigureAwait(false);
            return Response<bool>.Success(true);
        }
        catch (global::RaphCare.Client.Services.Base.ApiException ex)
        {
            return Response<bool>.Failure(ex.Message, ex.StatusCode);
        }
    }

    public async Task<Response<bool>> RegisterPushDeviceAsync(RegisterPatientPushDeviceRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var body = new RegisterMyPatientPushDeviceCommand
            {
                DeviceToken = request.DeviceToken,
                Platform = request.Platform,
            };
            await _client.RegisterMyPatientPushDeviceAsync(body, cancellationToken).ConfigureAwait(false);
            return Response<bool>.Success(true);
        }
        catch (global::RaphCare.Client.Services.Base.ApiException ex)
        {
            return Response<bool>.Failure(ex.Message, ex.StatusCode);
        }
    }

    private static PatientNotificationViewModel Map(PatientNotificationDto d) =>
        new()
        {
            Id = d.Id,
            Title = d.Title ?? string.Empty,
            Body = d.Body ?? string.Empty,
            Type = d.Type ?? string.Empty,
            IsRead = d.IsRead,
            CreatedAt = d.CreatedAt,
        };
}
