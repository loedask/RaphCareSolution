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
            var page = await client
                .GetFromJsonAsync<PagedClinicsDto>("api/admin/clinics?pageSize=100", JsonOptions, cancellationToken)
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
        catch (HttpRequestException ex)
        {
            return Response<IReadOnlyList<ClinicListItem>>.Failure(ex.Message);
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
            return Response<RegisterClinicResult>.Failure(error, (int)response.StatusCode);
        }
        catch (HttpRequestException ex)
        {
            return Response<RegisterClinicResult>.Failure(ex.Message);
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
