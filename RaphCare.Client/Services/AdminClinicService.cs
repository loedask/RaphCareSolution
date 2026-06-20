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

    public async Task<Response<ClinicDetail>> GetClinicAsync(Guid clinicId, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .GetAsync($"api/admin/clinics/{clinicId}", cancellationToken)
                .ConfigureAwait(false);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return Response<ClinicDetail>.Failure("Hospital not found or you do not have access.", 404);

            if (!response.IsSuccessStatusCode)
            {
                var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
                return Response<ClinicDetail>.Failure(error, (int)response.StatusCode);
            }

            var dto = await response.Content
                .ReadFromJsonAsync<ClinicDetailDto>(JsonOptions, cancellationToken)
                .ConfigureAwait(false);

            if (dto is null)
                return Response<ClinicDetail>.Failure("Could not load hospital.");

            return Response<ClinicDetail>.Success(MapDetail(dto));
        }
        catch (HttpRequestException)
        {
            return Response<ClinicDetail>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<bool>> EnsureMembershipAsync(Guid clinicId, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PostAsync($"api/admin/clinics/{clinicId}/membership", content: null, cancellationToken)
                .ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
                return Response<bool>.Success(true);

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                return Response<bool>.Failure("You cannot link this hospital to your account.", 403);

            var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
            return Response<bool>.Failure(error, (int)response.StatusCode);
        }
        catch (HttpRequestException)
        {
            return Response<bool>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<Guid?>> ClaimByRegistrationNumberAsync(
        string registrationNumber,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PostAsJsonAsync("api/admin/clinics/claim", new { registrationNumber }, cancellationToken)
                .ConfigureAwait(false);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return Response<Guid?>.Failure("No hospital found with that registration number, or it is already linked to another team.", 404);

            if (!response.IsSuccessStatusCode)
            {
                var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
                return Response<Guid?>.Failure(error, (int)response.StatusCode);
            }

            var dto = await response.Content
                .ReadFromJsonAsync<ClaimResultDto>(JsonOptions, cancellationToken)
                .ConfigureAwait(false);

            return Response<Guid?>.Success(dto?.ClinicId);
        }
        catch (HttpRequestException)
        {
            return Response<Guid?>.Failure("We couldn't reach the server. Check your connection and try again.");
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

    public async Task<Response<IReadOnlyList<ClinicStaffMember>>> GetStaffAsync(
        Guid clinicId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .GetAsync($"api/admin/clinics/{clinicId}/staff", cancellationToken)
                .ConfigureAwait(false);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return Response<IReadOnlyList<ClinicStaffMember>>.Failure("Hospital not found or you do not have access.", 404);

            if (!response.IsSuccessStatusCode)
            {
                var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
                return Response<IReadOnlyList<ClinicStaffMember>>.Failure(error, (int)response.StatusCode);
            }

            var items = await response.Content
                .ReadFromJsonAsync<List<ClinicStaffMemberDto>>(JsonOptions, cancellationToken)
                .ConfigureAwait(false);

            IReadOnlyList<ClinicStaffMember> staff = items?
                .Select(MapStaff)
                .ToList() ?? [];

            return Response<IReadOnlyList<ClinicStaffMember>>.Success(staff);
        }
        catch (HttpRequestException)
        {
            return Response<IReadOnlyList<ClinicStaffMember>>.Failure(
                "We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<ClinicStaffMember>> InviteStaffAsync(
        Guid clinicId,
        string email,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PostAsJsonAsync($"api/admin/clinics/{clinicId}/staff", new { email }, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    error = "Only hospital administrators can invite staff.";
                return Response<ClinicStaffMember>.Failure(error, (int)response.StatusCode);
            }

            var dto = await response.Content
                .ReadFromJsonAsync<ClinicStaffMemberDto>(JsonOptions, cancellationToken)
                .ConfigureAwait(false);

            if (dto is null)
                return Response<ClinicStaffMember>.Failure("Could not invite staff member.");

            return Response<ClinicStaffMember>.Success(MapStaff(dto));
        }
        catch (HttpRequestException)
        {
            return Response<ClinicStaffMember>.Failure(
                "We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<bool>> RemoveStaffAsync(
        Guid clinicId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .DeleteAsync($"api/admin/clinics/{clinicId}/staff/{userId}", cancellationToken)
                .ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
                return Response<bool>.Success(true);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return Response<bool>.Failure("Staff member not found.", 404);

            var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
            return Response<bool>.Failure(error, (int)response.StatusCode);
        }
        catch (HttpRequestException)
        {
            return Response<bool>.Failure(
                "We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<FacilityListItem>> CreateFacilityAsync(
        Guid clinicId,
        SaveFacilityRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PostAsJsonAsync($"api/admin/clinics/{clinicId}/facilities", request, cancellationToken)
                .ConfigureAwait(false);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return Response<FacilityListItem>.Failure("Hospital not found or you do not have access.", 404);

            if (!response.IsSuccessStatusCode)
            {
                var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
                return Response<FacilityListItem>.Failure(error, (int)response.StatusCode);
            }

            var dto = await response.Content
                .ReadFromJsonAsync<FacilityListItemDto>(JsonOptions, cancellationToken)
                .ConfigureAwait(false);

            if (dto is null)
                return Response<FacilityListItem>.Failure("Could not create facility.");

            return Response<FacilityListItem>.Success(MapFacility(dto));
        }
        catch (HttpRequestException)
        {
            return Response<FacilityListItem>.Failure(
                "We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<FacilityListItem>> UpdateFacilityAsync(
        Guid clinicId,
        Guid facilityId,
        SaveFacilityRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PutAsJsonAsync($"api/admin/clinics/{clinicId}/facilities/{facilityId}", request, cancellationToken)
                .ConfigureAwait(false);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return Response<FacilityListItem>.Failure("Facility not found.", 404);

            if (!response.IsSuccessStatusCode)
            {
                var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
                return Response<FacilityListItem>.Failure(error, (int)response.StatusCode);
            }

            var dto = await response.Content
                .ReadFromJsonAsync<FacilityListItemDto>(JsonOptions, cancellationToken)
                .ConfigureAwait(false);

            if (dto is null)
                return Response<FacilityListItem>.Failure("Could not update facility.");

            return Response<FacilityListItem>.Success(MapFacility(dto));
        }
        catch (HttpRequestException)
        {
            return Response<FacilityListItem>.Failure(
                "We couldn't reach the server. Check your connection and try again.");
        }
    }

    private static ClinicStaffMember MapStaff(ClinicStaffMemberDto dto) => new()
    {
        UserId = dto.UserId,
        Email = dto.Email,
        DisplayName = dto.DisplayName,
        Roles = dto.Roles ?? [],
        JoinedAt = dto.JoinedAt,
        IsActive = dto.IsActive
    };

    private static FacilityListItem MapFacility(FacilityListItemDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        Address = dto.Address,
        City = dto.City,
        Country = dto.Country,
        IsVirtual = dto.IsVirtual
    };

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
                if (!string.IsNullOrWhiteSpace(dto?.Detail))
                    return dto.Detail;
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

    private static ClinicDetail MapDetail(ClinicDetailDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        RegistrationNumber = dto.RegistrationNumber,
        Country = dto.Country,
        TimeZone = dto.TimeZone,
        IsActive = dto.IsActive,
        CreatedAt = dto.CreatedAt,
        Facilities = dto.Facilities?
            .Select(f => new FacilityListItem
            {
                Id = f.Id,
                Name = f.Name,
                Address = f.Address,
                City = f.City,
                Country = f.Country,
                IsVirtual = f.IsVirtual
            })
            .ToList() ?? []
    };

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

    private sealed class ClinicDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string TimeZone { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<FacilityListItemDto>? Facilities { get; set; }
    }

    private sealed class FacilityListItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public bool IsVirtual { get; set; }
    }

    private sealed class ClaimResultDto
    {
        public Guid ClinicId { get; set; }
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
        public string? Detail { get; set; }
    }

    private sealed class ClinicStaffMemberDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public List<string>? Roles { get; set; }
        public DateTime JoinedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
