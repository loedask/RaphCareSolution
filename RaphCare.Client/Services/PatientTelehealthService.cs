using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Telehealth;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client.Services;

public class PatientTelehealthService(IClient client, HttpClient httpClient) : BaseHttpService(client, httpClient), IPatientTelehealthService
{
    public Task<Response<PagedPatientTeleSessionsViewModel>> GetMySessionsAsync(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default) =>
        GetAsync<PagedPatientTeleSessionsViewModel>($"api/patient/telehealth/sessions?pageNumber={pageNumber}&pageSize={pageSize}", cancellationToken);

    public Task<Response<TelehealthJoinInfoViewModel?>> GetJoinInfoAsync(Guid teleSessionId, uint? uid = null, CancellationToken cancellationToken = default)
    {
        var uri = uid is null
            ? $"api/patient/telehealth/sessions/{teleSessionId}/join-info"
            : $"api/patient/telehealth/sessions/{teleSessionId}/join-info?uid={uid.Value}";
        return GetAsync<TelehealthJoinInfoViewModel?>(uri, cancellationToken);
    }

    public async Task<Response<bool>> SendSessionSmsAsync(Guid teleSessionId, CancellationToken cancellationToken = default)
    {
        using var response = await HttpClient.PostAsync($"api/patient/telehealth/sessions/{teleSessionId}/notify-sms", null, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            return Response<bool>.Failure(string.IsNullOrEmpty(body) ? $"SMS request failed: {response.StatusCode}" : body, (int)response.StatusCode);
        }

        return Response<bool>.Success(true);
    }
}
