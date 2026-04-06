using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Notifications;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client.Services;

/// <summary>HTTP access to <c>api/patient/notifications</c> until operations exist on <see cref="IClient"/> (NSwag regen).</summary>
public sealed class PatientNotificationsService(IClient client, HttpClient httpClient) : BaseHttpService(client, httpClient), IPatientNotificationsService
{
    private const string BaseUri = "api/patient/notifications";

    public async Task<Response<IReadOnlyList<PatientNotificationViewModel>>> GetMyNotificationsAsync(CancellationToken cancellationToken = default)
    {
        var listResponse = await GetAsync<List<PatientNotificationViewModel>>(BaseUri, cancellationToken).ConfigureAwait(false);
        if (!listResponse.IsSuccess || listResponse.Data is null)
            return Response<IReadOnlyList<PatientNotificationViewModel>>.Failure(listResponse.ErrorMessage ?? "Request failed", listResponse.StatusCode);
        return Response<IReadOnlyList<PatientNotificationViewModel>>.Success(listResponse.Data);
    }

    public Task<Response<bool>> MarkReadAsync(Guid id, CancellationToken cancellationToken = default) =>
        PutEmptyAsync($"{BaseUri}/{id}/read", cancellationToken);

    public Task<Response<bool>> MarkAllReadAsync(CancellationToken cancellationToken = default) =>
        PutEmptyAsync($"{BaseUri}/read-all", cancellationToken);

    public Task<Response<bool>> RegisterPushDeviceAsync(RegisterPatientPushDeviceRequest request, CancellationToken cancellationToken = default) =>
        PutAsJsonNoContentAsync($"{BaseUri}/push-device", request, cancellationToken);
}
