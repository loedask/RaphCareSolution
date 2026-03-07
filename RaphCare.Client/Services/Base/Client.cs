using System.Net.Http.Json;
using System.Text.Json;
using RaphCare.Client.Contracts;

namespace RaphCare.Client.Services.Base;

/// <summary>
/// Partial class for extending the NSwag-generated API client. Implements typed patient methods that call the API and return DTOs.
/// </summary>
public partial class Client : IClient
{
    public HttpClient HttpClient => _httpClient;

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task<PatientDto?> GetPatientAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/Patients/{id}", cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            await ThrowApiExceptionAsync(response, cancellationToken).ConfigureAwait(false);
        return await response.Content.ReadFromJsonAsync<PatientDto>(JsonOptions, cancellationToken).ConfigureAwait(false);
    }

    public async Task<PagedResultOfPatientDto?> GetPatientsAsync(int? pageNumber = null, int? pageSize = null, CancellationToken cancellationToken = default)
    {
        var query = new List<string>();
        if (pageNumber.HasValue) query.Add($"pageNumber={pageNumber.Value}");
        if (pageSize.HasValue) query.Add($"pageSize={pageSize.Value}");
        var path = query.Count == 0 ? "api/Patients" : "api/Patients?" + string.Join("&", query);
        var response = await _httpClient.GetAsync(path, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            await ThrowApiExceptionAsync(response, cancellationToken).ConfigureAwait(false);
        return await response.Content.ReadFromJsonAsync<PagedResultOfPatientDto>(JsonOptions, cancellationToken).ConfigureAwait(false);
    }

    public async Task<CreatePatientResult?> CreatePatientAsync(CreatePatientCommand? body = null, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/Patients", body, JsonOptions, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            await ThrowApiExceptionAsync(response, cancellationToken).ConfigureAwait(false);
        return await response.Content.ReadFromJsonAsync<CreatePatientResult>(JsonOptions, cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdatePatientAsync(Guid id, UpdatePatientCommand? body = null, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/Patients/{id}", body, JsonOptions, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            await ThrowApiExceptionAsync(response, cancellationToken).ConfigureAwait(false);
    }

    private static async Task ThrowApiExceptionAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        throw new Contracts.ApiException($"API error: {response.StatusCode}.", (int)response.StatusCode, body);
    }
}
