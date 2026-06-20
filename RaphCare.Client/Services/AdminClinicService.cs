using System.Net.Http.Json;
using System.Text.Json;
using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models;

namespace RaphCare.Client.Services;

public sealed class AdminClinicService(IHttpClientFactory httpClientFactory) : IAdminClinicService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<Response<IReadOnlyList<ClinicListItem>>> GetClinicsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .GetAsync("api/admin/clinics?pageSize=100", cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
                error = MapStatusToMessage((int)response.StatusCode, error, forList: true);
                return Response<IReadOnlyList<ClinicListItem>>.Failure(error, (int)response.StatusCode);
            }

            var page = await response.Content
                .ReadFromJsonAsync<PagedClinicsDto>(JsonOptions, cancellationToken)
                .ConfigureAwait(false);

            if (page?.Items is null)
                return Response<IReadOnlyList<ClinicListItem>>.Failure("Could not load hospitals.");

            IReadOnlyList<ClinicListItem> items = page.Items
                .Select(c => new ClinicListItem
                {
                    Id = c.Id,
                    Name = c.Name,
                    RegistrationNumber = c.RegistrationNumber,
                    Country = c.Country,
                    TimeZone = c.TimeZone,
                    IsActive = c.IsActive,
                    FacilityCount = c.FacilityCount,
                    CreatedAt = c.CreatedAt
                })
                .ToList();

            return Response<IReadOnlyList<ClinicListItem>>.Success(items);
        }
        catch (HttpRequestException)
        {
            return Response<IReadOnlyList<ClinicListItem>>.Failure(
                "We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<RegisterClinicResult>> RegisterClinicAsync(
        RegisterClinicRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PostAsJsonAsync("api/admin/clinics", request, cancellationToken)
                .ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var dto = await response.Content
                    .ReadFromJsonAsync<RegisterClinicResultDto>(JsonOptions, cancellationToken)
                    .ConfigureAwait(false);

                if (dto is null)
                    return Response<RegisterClinicResult>.Failure("Empty response from server.");

                return Response<RegisterClinicResult>.Success(new RegisterClinicResult
                {
                    ClinicId = dto.ClinicId,
                    Name = dto.Name,
                    PrimaryFacilityId = dto.PrimaryFacilityId,
                    PrimaryFacilityName = dto.PrimaryFacilityName
                });
            }

            var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
            error = MapStatusToMessage((int)response.StatusCode, error);
            return Response<RegisterClinicResult>.Failure(error, (int)response.StatusCode);
        }
        catch (HttpRequestException)
        {
            return Response<RegisterClinicResult>.Failure(
                "We couldn't reach the server. Check your connection and try again.");
        }
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var body = response.Content is null
            ? null
            : await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(body))
        {
            try
            {
                var dto = JsonSerializer.Deserialize<ErrorDto>(body, JsonOptions);
                if (!string.IsNullOrWhiteSpace(dto?.Error))
                    return dto.Error;
            }
            catch
            {
            }
        }

        return response.ReasonPhrase ?? "Request failed.";
    }

    private static string MapStatusToMessage(int statusCode, string fallback, bool forList = false) =>
        statusCode switch
        {
            401 when forList =>
                "Your session expired or is invalid. Sign in again to view hospitals.",
            401 =>
                "Your sign-in session expired. Use Sign in and continue below, then tap Register hospital again.",
            403 when forList =>
                "You don't have permission to view hospitals. Sign in with a healthcare professional account.",
            403 =>
                "You do not have permission to register a hospital. Sign in with a professional account.",
            _ => HumanizeFallback(fallback, statusCode)
        };

    private static string HumanizeFallback(string fallback, int statusCode) =>
        fallback.Contains("net_http_message_not_success", StringComparison.OrdinalIgnoreCase)
        || fallback.Contains("Unauthorized", StringComparison.OrdinalIgnoreCase) && statusCode == 401
            ? "Something went wrong. Sign in again or try later."
            : fallback;

    private sealed class PagedClinicsDto
    {
        public List<ClinicListItemDto>? Items { get; set; }
    }

    private sealed class ClinicListItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string TimeZone { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int FacilityCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    private sealed class RegisterClinicResultDto
    {
        public Guid ClinicId { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid? PrimaryFacilityId { get; set; }
        public string? PrimaryFacilityName { get; set; }
    }

    private sealed class ErrorDto
    {
        public string? Error { get; set; }
    }
}
