using System.Net.Http.Json;
using System.Text.Json;
using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.MentalHealth;

namespace RaphCare.Client.Services;

/// <summary>Calls patient mental health endpoints. Replace with generated <see cref="Base.IClient"/> after NSwag regen.</summary>
public sealed class PatientMentalHealthService(IHttpClientFactory httpClientFactory) : IPatientMentalHealthService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public async Task<Response<PatientMentalHealthContentViewModel>> GetContentAsync(CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
        using var response = await client.GetAsync(new Uri("api/patient/mental-health/content", UriKind.Relative), cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            var msg = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            return Response<PatientMentalHealthContentViewModel>.Failure(
                string.IsNullOrWhiteSpace(msg) ? response.ReasonPhrase ?? "Request failed." : msg,
                (int)response.StatusCode);
        }

        var dto = await response.Content.ReadFromJsonAsync<PatientMentalHealthContentViewModel>(JsonOptions, cancellationToken).ConfigureAwait(false);
        if (dto is null)
            return Response<PatientMentalHealthContentViewModel>.Failure("Empty response.", (int)response.StatusCode);
        return Response<PatientMentalHealthContentViewModel>.Success(dto);
    }

    public async Task<Response<Guid>> LogMoodCheckInAsync(int moodScore, string? notes, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
        using var response = await client.PostAsJsonAsync(
                new Uri("api/patient/mental-health/mood-checkin", UriKind.Relative),
                new { moodScore, notes },
                JsonOptions,
                cancellationToken)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var msg = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            return Response<Guid>.Failure(
                string.IsNullOrWhiteSpace(msg) ? response.ReasonPhrase ?? "Request failed." : msg,
                (int)response.StatusCode);
        }

        var body = await response.Content.ReadFromJsonAsync<CreatedGuidJson>(JsonOptions, cancellationToken).ConfigureAwait(false);
        if (body is null)
            return Response<Guid>.Failure("Empty response.", (int)response.StatusCode);
        return Response<Guid>.Success(body.Id);
    }

    private sealed class CreatedGuidJson
    {
        public Guid Id { get; set; }
    }
}
