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

    public async Task<Response<ClinicDetail>> UpdateClinicAsync(
        Guid clinicId,
        UpdateClinicRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PutAsJsonAsync($"api/admin/clinics/{clinicId}", request, cancellationToken)
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
                return Response<ClinicDetail>.Failure("Could not update hospital.");

            return Response<ClinicDetail>.Success(MapDetail(dto));
        }
        catch (HttpRequestException)
        {
            return Response<ClinicDetail>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<PagedClinicPatients>> GetPatientsAsync(
        Guid clinicId,
        int pageNumber = 1,
        int pageSize = 20,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            var query = $"api/admin/clinics/{clinicId}/patients?pageNumber={pageNumber}&pageSize={pageSize}";
            if (!string.IsNullOrWhiteSpace(search))
                query += $"&search={Uri.EscapeDataString(search.Trim())}";

            using var response = await client
                .GetAsync(query, cancellationToken)
                .ConfigureAwait(false);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return Response<PagedClinicPatients>.Failure("Hospital not found or you do not have access.", 404);

            if (!response.IsSuccessStatusCode)
            {
                var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
                return Response<PagedClinicPatients>.Failure(error, (int)response.StatusCode);
            }

            var page = await response.Content
                .ReadFromJsonAsync<PagedPatientsDto>(JsonOptions, cancellationToken)
                .ConfigureAwait(false);

            if (page?.Items is null)
                return Response<PagedClinicPatients>.Failure("Could not load patients.");

            return Response<PagedClinicPatients>.Success(new PagedClinicPatients
            {
                Items = page.Items.Select(p => new ClinicPatientListItem
                {
                    PatientId = p.PatientId,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    DateOfBirth = p.DateOfBirth,
                    AccessType = p.AccessType,
                    GrantedAt = p.GrantedAt
                }).ToList(),
                TotalCount = page.TotalCount,
                PageNumber = page.PageNumber,
                PageSize = page.PageSize
            });
        }
        catch (HttpRequestException)
        {
            return Response<PagedClinicPatients>.Failure(
                "We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<ClinicPatientDetail>> GetPatientDetailAsync(
        Guid clinicId,
        Guid patientId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .GetAsync($"api/admin/clinics/{clinicId}/patients/{patientId}", cancellationToken)
                .ConfigureAwait(false);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return Response<ClinicPatientDetail>.Failure("Patient not found or you do not have access.", 404);

            if (!response.IsSuccessStatusCode)
            {
                var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
                return Response<ClinicPatientDetail>.Failure(error, (int)response.StatusCode);
            }

            var dto = await response.Content
                .ReadFromJsonAsync<PatientDetailDto>(JsonOptions, cancellationToken)
                .ConfigureAwait(false);

            if (dto is null)
                return Response<ClinicPatientDetail>.Failure("Could not load patient.");

            return Response<ClinicPatientDetail>.Success(MapPatientDetail(dto));
        }
        catch (HttpRequestException)
        {
            return Response<ClinicPatientDetail>.Failure(
                "We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<ClinicPatientListItem>> GrantPatientAccessAsync(
        Guid clinicId,
        string email,
        string? notes = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PostAsJsonAsync($"api/admin/clinics/{clinicId}/patients", new { email, notes }, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    error = "Only hospital administrators can grant patient access.";
                return Response<ClinicPatientListItem>.Failure(error, (int)response.StatusCode);
            }

            var dto = await response.Content
                .ReadFromJsonAsync<PatientListItemDto>(JsonOptions, cancellationToken)
                .ConfigureAwait(false);

            if (dto is null)
                return Response<ClinicPatientListItem>.Failure("Could not grant patient access.");

            return Response<ClinicPatientListItem>.Success(new ClinicPatientListItem
            {
                PatientId = dto.PatientId,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                DateOfBirth = dto.DateOfBirth,
                AccessType = dto.AccessType,
                GrantedAt = dto.GrantedAt
            });
        }
        catch (HttpRequestException)
        {
            return Response<ClinicPatientListItem>.Failure(
                "We couldn't reach the server. Check your connection and try again.");
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

    public async Task<Response<bool>> ResendStaffInvitationAsync(
        Guid clinicId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PostAsync($"api/admin/clinics/{clinicId}/staff/{userId}/resend-invitation", content: null, cancellationToken)
                .ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
                return Response<bool>.Success(true);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return Response<bool>.Failure("Staff member not found.", 404);

            var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                error = "Only hospital administrators can resend invitations.";
            return Response<bool>.Failure(error, (int)response.StatusCode);
        }
        catch (HttpRequestException)
        {
            return Response<bool>.Failure(
                "We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<bool>> ResendPendingInvitationAsync(
        Guid clinicId,
        Guid invitationId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PostAsync($"api/admin/clinics/{clinicId}/staff/invitations/{invitationId}/resend", content: null, cancellationToken)
                .ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
                return Response<bool>.Success(true);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return Response<bool>.Failure("Pending invitation not found.", 404);

            var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                error = "Only hospital administrators can resend invitations.";
            return Response<bool>.Failure(error, (int)response.StatusCode);
        }
        catch (HttpRequestException)
        {
            return Response<bool>.Failure(
                "We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<bool>> CancelPendingInvitationAsync(
        Guid clinicId,
        Guid invitationId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .DeleteAsync($"api/admin/clinics/{clinicId}/staff/invitations/{invitationId}", cancellationToken)
                .ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
                return Response<bool>.Success(true);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return Response<bool>.Failure("Pending invitation not found.", 404);

            var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
            return Response<bool>.Failure(error, (int)response.StatusCode);
        }
        catch (HttpRequestException)
        {
            return Response<bool>.Failure(
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

    public async Task<Response<bool>> UpdateStaffRoleAsync(
        Guid clinicId,
        Guid userId,
        bool isAdministrator,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PutAsJsonAsync($"api/admin/clinics/{clinicId}/staff/{userId}/role", new { isAdministrator }, cancellationToken)
                .ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
                return Response<bool>.Success(true);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return Response<bool>.Failure("Staff member not found.", 404);

            var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                error = "Only hospital administrators can change staff roles.";
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

    public async Task<Response<bool>> DeleteFacilityAsync(
        Guid clinicId,
        Guid facilityId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .DeleteAsync($"api/admin/clinics/{clinicId}/facilities/{facilityId}", cancellationToken)
                .ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
                return Response<bool>.Success(true);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return Response<bool>.Failure("Facility not found.", 404);

            var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                error = "Only hospital administrators can delete facilities.";
            return Response<bool>.Failure(error, (int)response.StatusCode);
        }
        catch (HttpRequestException)
        {
            return Response<bool>.Failure(
                "We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<ClinicDashboard>> GetDashboardAsync(Guid clinicId, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client.GetAsync($"api/admin/clinics/{clinicId}/dashboard", cancellationToken).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return Response<ClinicDashboard>.Failure("Hospital not found or you do not have access.", 404);
            if (!response.IsSuccessStatusCode)
                return Response<ClinicDashboard>.Failure(await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false), (int)response.StatusCode);
            var dto = await response.Content.ReadFromJsonAsync<DashboardDto>(JsonOptions, cancellationToken).ConfigureAwait(false);
            if (dto is null) return Response<ClinicDashboard>.Failure("Could not load dashboard.");
            return Response<ClinicDashboard>.Success(MapDashboard(dto));
        }
        catch (HttpRequestException)
        {
            return Response<ClinicDashboard>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<IReadOnlyList<ClinicProviderListItem>>> GetProvidersAsync(Guid clinicId, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client.GetAsync($"api/admin/clinics/{clinicId}/providers", cancellationToken).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return Response<IReadOnlyList<ClinicProviderListItem>>.Failure("Hospital not found or you do not have access.", 404);
            if (!response.IsSuccessStatusCode)
                return Response<IReadOnlyList<ClinicProviderListItem>>.Failure(await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false), (int)response.StatusCode);
            var items = await response.Content.ReadFromJsonAsync<List<ProviderListItemDto>>(JsonOptions, cancellationToken).ConfigureAwait(false);
            IReadOnlyList<ClinicProviderListItem> providers = items?.Select(MapProvider).ToList() ?? [];
            return Response<IReadOnlyList<ClinicProviderListItem>>.Success(providers);
        }
        catch (HttpRequestException)
        {
            return Response<IReadOnlyList<ClinicProviderListItem>>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<ClinicProviderDetail>> GetProviderDetailAsync(Guid clinicId, Guid providerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client.GetAsync($"api/admin/clinics/{clinicId}/providers/{providerId}", cancellationToken).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return Response<ClinicProviderDetail>.Failure("Provider not found.", 404);
            if (!response.IsSuccessStatusCode)
                return Response<ClinicProviderDetail>.Failure(await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false), (int)response.StatusCode);
            var dto = await response.Content.ReadFromJsonAsync<ProviderDetailDto>(JsonOptions, cancellationToken).ConfigureAwait(false);
            if (dto is null) return Response<ClinicProviderDetail>.Failure("Could not load provider.");
            return Response<ClinicProviderDetail>.Success(MapProviderDetail(dto));
        }
        catch (HttpRequestException)
        {
            return Response<ClinicProviderDetail>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<ClinicProviderListItem>> CreateProviderAsync(Guid clinicId, Guid userId, string? licenseNumber = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client.PostAsJsonAsync($"api/admin/clinics/{clinicId}/providers", new { userId, licenseNumber }, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    error = "Only hospital administrators can add providers.";
                return Response<ClinicProviderListItem>.Failure(error, (int)response.StatusCode);
            }
            var dto = await response.Content.ReadFromJsonAsync<ProviderListItemDto>(JsonOptions, cancellationToken).ConfigureAwait(false);
            if (dto is null) return Response<ClinicProviderListItem>.Failure("Could not add provider.");
            return Response<ClinicProviderListItem>.Success(MapProvider(dto));
        }
        catch (HttpRequestException)
        {
            return Response<ClinicProviderListItem>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<ClinicProviderSchedule>> CreateProviderScheduleAsync(Guid clinicId, Guid providerId, CreateProviderScheduleRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client.PostAsJsonAsync($"api/admin/clinics/{clinicId}/providers/{providerId}/schedules", request, cancellationToken).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return Response<ClinicProviderSchedule>.Failure("Provider not found.", 404);
            if (!response.IsSuccessStatusCode)
                return Response<ClinicProviderSchedule>.Failure(await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false), (int)response.StatusCode);
            var dto = await response.Content.ReadFromJsonAsync<ProviderScheduleDto>(JsonOptions, cancellationToken).ConfigureAwait(false);
            if (dto is null) return Response<ClinicProviderSchedule>.Failure("Could not add schedule slot.");
            return Response<ClinicProviderSchedule>.Success(MapSchedule(dto));
        }
        catch (HttpRequestException)
        {
            return Response<ClinicProviderSchedule>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<bool>> DeleteProviderScheduleAsync(Guid clinicId, Guid providerId, Guid scheduleId, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client.DeleteAsync($"api/admin/clinics/{clinicId}/providers/{providerId}/schedules/{scheduleId}", cancellationToken).ConfigureAwait(false);
            if (response.IsSuccessStatusCode) return Response<bool>.Success(true);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return Response<bool>.Failure("Schedule slot not found.", 404);
            return Response<bool>.Failure(await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false), (int)response.StatusCode);
        }
        catch (HttpRequestException)
        {
            return Response<bool>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<PagedClinicAppointments>> GetAppointmentsAsync(
        Guid clinicId,
        int pageNumber = 1,
        int pageSize = 20,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            var query = $"api/admin/clinics/{clinicId}/appointments?pageNumber={pageNumber}&pageSize={pageSize}";
            if (fromUtc is { } from) query += $"&fromUtc={Uri.EscapeDataString(from.ToString("O"))}";
            if (toUtc is { } to) query += $"&toUtc={Uri.EscapeDataString(to.ToString("O"))}";
            if (!string.IsNullOrWhiteSpace(status)) query += $"&status={Uri.EscapeDataString(status.Trim())}";
            using var response = await client.GetAsync(query, cancellationToken).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return Response<PagedClinicAppointments>.Failure("Hospital not found or you do not have access.", 404);
            if (!response.IsSuccessStatusCode)
                return Response<PagedClinicAppointments>.Failure(await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false), (int)response.StatusCode);
            var page = await response.Content.ReadFromJsonAsync<PagedAppointmentsDto>(JsonOptions, cancellationToken).ConfigureAwait(false);
            if (page?.Items is null) return Response<PagedClinicAppointments>.Failure("Could not load appointments.");
            return Response<PagedClinicAppointments>.Success(new PagedClinicAppointments
            {
                Items = page.Items.Select(MapAppointment).ToList(),
                TotalCount = page.TotalCount,
                PageNumber = page.PageNumber,
                PageSize = page.PageSize
            });
        }
        catch (HttpRequestException)
        {
            return Response<PagedClinicAppointments>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<ClinicAppointmentListItem>> BookAppointmentAsync(
        Guid clinicId,
        BookClinicAppointmentRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PostAsJsonAsync($"api/admin/clinics/{clinicId}/appointments", request, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    error = "Only hospital administrators can book appointments.";
                return Response<ClinicAppointmentListItem>.Failure(error, (int)response.StatusCode);
            }

            var dto = await response.Content.ReadFromJsonAsync<AppointmentListItemDto>(JsonOptions, cancellationToken).ConfigureAwait(false);
            if (dto is null)
                return Response<ClinicAppointmentListItem>.Failure("Could not book appointment.");

            return Response<ClinicAppointmentListItem>.Success(MapAppointment(dto));
        }
        catch (HttpRequestException)
        {
            return Response<ClinicAppointmentListItem>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<bool>> CancelAppointmentAsync(
        Guid clinicId,
        Guid appointmentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .DeleteAsync($"api/admin/clinics/{clinicId}/appointments/{appointmentId}", cancellationToken)
                .ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
                return Response<bool>.Success(true);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return Response<bool>.Failure("Appointment not found.", 404);

            var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                error = "Only hospital administrators can cancel appointments.";
            return Response<bool>.Failure(error, (int)response.StatusCode);
        }
        catch (HttpRequestException)
        {
            return Response<bool>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<ClinicAppointmentListItem>> RescheduleAppointmentAsync(
        Guid clinicId,
        Guid appointmentId,
        RescheduleClinicAppointmentRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PutAsJsonAsync($"api/admin/clinics/{clinicId}/appointments/{appointmentId}", request, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    error = "Only hospital administrators can reschedule appointments.";
                return Response<ClinicAppointmentListItem>.Failure(error, (int)response.StatusCode);
            }

            var dto = await response.Content.ReadFromJsonAsync<AppointmentListItemDto>(JsonOptions, cancellationToken).ConfigureAwait(false);
            if (dto is null)
                return Response<ClinicAppointmentListItem>.Failure("Could not reschedule appointment.");

            return Response<ClinicAppointmentListItem>.Success(MapAppointment(dto));
        }
        catch (HttpRequestException)
        {
            return Response<ClinicAppointmentListItem>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<bool>> RevokePatientAccessAsync(
        Guid clinicId,
        Guid patientId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .DeleteAsync($"api/admin/clinics/{clinicId}/patients/{patientId}", cancellationToken)
                .ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
                return Response<bool>.Success(true);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return Response<bool>.Failure("Patient not found or access already revoked.", 404);

            var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                error = "Only hospital administrators can revoke patient access.";
            return Response<bool>.Failure(error, (int)response.StatusCode);
        }
        catch (HttpRequestException)
        {
            return Response<bool>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    private static ClinicStaffMember MapStaff(ClinicStaffMemberDto dto) => new()
    {
        UserId = dto.UserId,
        InvitationId = dto.InvitationId,
        IsPendingInvitation = dto.IsPendingInvitation,
        Email = dto.Email,
        DisplayName = dto.DisplayName,
        Roles = dto.Roles ?? [],
        JoinedAt = dto.JoinedAt,
        IsActive = dto.IsActive,
        HasLoggedIn = dto.HasLoggedIn,
        LastInvitationSentAt = dto.LastInvitationSentAt
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
        RegisteredByApplicationUserId = dto.RegisteredByApplicationUserId,
        CurrentUserIsAdministrator = dto.CurrentUserIsAdministrator,
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

    private static ClinicPatientDetail MapPatientDetail(PatientDetailDto dto) => new()
    {
        PatientId = dto.PatientId,
        FirstName = dto.FirstName,
        LastName = dto.LastName,
        DateOfBirth = dto.DateOfBirth,
        Gender = dto.Gender,
        Email = dto.Email,
        PhoneNumber = dto.PhoneNumber,
        AccessType = dto.AccessType,
        GrantedAt = dto.GrantedAt,
        GrantedByRule = dto.GrantedByRule,
        Notes = dto.Notes,
            RecentVisits = dto.RecentVisits?
            .Select(v => new ClinicPatientVisitSummary
            {
                Id = v.Id,
                VisitStart = v.VisitStart,
                VisitEnd = v.VisitEnd,
                VisitType = v.VisitType,
                Status = v.Status,
                Summary = v.Summary
            })
            .ToList() ?? [],
            Appointments = dto.Appointments?
            .Select(a => new ClinicPatientAppointmentSummary
            {
                Id = a.Id,
                ScheduledStart = a.ScheduledStart,
                ScheduledEnd = a.ScheduledEnd,
                Type = a.Type,
                Status = a.Status,
                Reason = a.Reason,
                ProviderName = a.ProviderName
            })
            .ToList() ?? [],
            RecentVitals = dto.RecentVitals?
            .Select(v => new ClinicPatientVitalSummary
            {
                Type = v.Type,
                Value = v.Value,
                Unit = v.Unit,
                RecordedAt = v.RecordedAt,
                VisitType = v.VisitType
            })
            .ToList() ?? [],
            RecentDeviceReadings = dto.RecentDeviceReadings?
            .Select(r => new ClinicPatientDeviceReadingSummary
            {
                Kind = r.Kind,
                ReadingType = r.ReadingType,
                PrimaryValue = r.PrimaryValue,
                Unit = r.Unit,
                RecordedAt = r.RecordedAt,
                HeartRateBpm = r.HeartRateBpm,
                SpO2Percent = r.SpO2Percent
            })
            .ToList() ?? [],
            DeviceDailyRollups = dto.DeviceDailyRollups?
            .Select(r => new ClinicPatientDeviceRollupSummary
            {
                Date = r.Date,
                HeartRateSampleCount = r.HeartRateSampleCount,
                AvgHeartRateBpm = r.AvgHeartRateBpm,
                MinHeartRateBpm = r.MinHeartRateBpm,
                MaxHeartRateBpm = r.MaxHeartRateBpm,
                SpO2SampleCount = r.SpO2SampleCount,
                AvgSpO2Percent = r.AvgSpO2Percent,
                MinSpO2Percent = r.MinSpO2Percent,
                MaxSpO2Percent = r.MaxSpO2Percent
            })
            .ToList() ?? []
    };

    private static ClinicDashboard MapDashboard(DashboardDto dto) => new()
    {
        ClinicId = dto.ClinicId,
        ClinicName = dto.ClinicName,
        PatientCount = dto.PatientCount,
        StaffCount = dto.StaffCount,
        ProviderCount = dto.ProviderCount,
        FacilityCount = dto.FacilityCount,
        AppointmentsTodayCount = dto.AppointmentsTodayCount,
        UpcomingAppointmentsCount = dto.UpcomingAppointmentsCount,
        UpcomingAppointments = dto.UpcomingAppointments?.Select(MapAppointment).ToList() ?? []
    };

    private static ClinicProviderListItem MapProvider(ProviderListItemDto dto) => new()
    {
        ProviderId = dto.ProviderId,
        ApplicationUserId = dto.ApplicationUserId,
        DisplayName = dto.DisplayName,
        Email = dto.Email,
        LicenseNumber = dto.LicenseNumber,
        IsActive = dto.IsActive,
        ScheduleSlotCount = dto.ScheduleSlotCount
    };

    private static ClinicProviderDetail MapProviderDetail(ProviderDetailDto dto) => new()
    {
        ProviderId = dto.ProviderId,
        ApplicationUserId = dto.ApplicationUserId,
        DisplayName = dto.DisplayName,
        Email = dto.Email,
        LicenseNumber = dto.LicenseNumber,
        IsActive = dto.IsActive,
        Schedules = dto.Schedules?.Select(MapSchedule).ToList() ?? []
    };

    private static ClinicProviderSchedule MapSchedule(ProviderScheduleDto dto) => new()
    {
        Id = dto.Id,
        Day = dto.Day,
        StartTime = dto.StartTime,
        EndTime = dto.EndTime,
        IsRecurring = dto.IsRecurring
    };

    private static ClinicAppointmentListItem MapAppointment(AppointmentListItemDto dto) => new()
    {
        Id = dto.Id,
        PatientId = dto.PatientId,
        PatientName = dto.PatientName,
        ProviderId = dto.ProviderId,
        ProviderName = dto.ProviderName,
        ScheduledStart = dto.ScheduledStart,
        ScheduledEnd = dto.ScheduledEnd,
        Type = dto.Type,
        Status = dto.Status,
        Reason = dto.Reason
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
        public Guid? RegisteredByApplicationUserId { get; set; }
        public bool CurrentUserIsAdministrator { get; set; }
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
        public Guid? UserId { get; set; }
        public Guid? InvitationId { get; set; }
        public bool IsPendingInvitation { get; set; }
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public List<string>? Roles { get; set; }
        public DateTime JoinedAt { get; set; }
        public bool IsActive { get; set; }
        public bool HasLoggedIn { get; set; }
        public DateTime? LastInvitationSentAt { get; set; }
    }

    private sealed class PagedPatientsDto
    {
        public List<PatientListItemDto>? Items { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    private sealed class PatientListItemDto
    {
        public Guid PatientId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string AccessType { get; set; } = string.Empty;
        public DateTime GrantedAt { get; set; }
    }

    private sealed class PatientDetailDto
    {
        public Guid PatientId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string AccessType { get; set; } = string.Empty;
        public DateTime GrantedAt { get; set; }
        public string GrantedByRule { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public List<PatientVisitDto>? RecentVisits { get; set; }
        public List<PatientAppointmentDto>? Appointments { get; set; }
        public List<PatientVitalDto>? RecentVitals { get; set; }
        public List<PatientDeviceReadingDto>? RecentDeviceReadings { get; set; }
        public List<PatientDeviceRollupDto>? DeviceDailyRollups { get; set; }
    }

    private sealed class PatientVitalDto
    {
        public string Type { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public DateTime RecordedAt { get; set; }
        public string? VisitType { get; set; }
    }

    private sealed class PatientDeviceReadingDto
    {
        public string Kind { get; set; } = string.Empty;
        public string ReadingType { get; set; } = string.Empty;
        public decimal PrimaryValue { get; set; }
        public string Unit { get; set; } = string.Empty;
        public DateTime RecordedAt { get; set; }
        public int? HeartRateBpm { get; set; }
        public decimal? SpO2Percent { get; set; }
    }

    private sealed class PatientDeviceRollupDto
    {
        public DateOnly Date { get; set; }
        public int HeartRateSampleCount { get; set; }
        public decimal? AvgHeartRateBpm { get; set; }
        public int? MinHeartRateBpm { get; set; }
        public int? MaxHeartRateBpm { get; set; }
        public int SpO2SampleCount { get; set; }
        public decimal? AvgSpO2Percent { get; set; }
        public decimal? MinSpO2Percent { get; set; }
        public decimal? MaxSpO2Percent { get; set; }
    }

    private sealed class PatientAppointmentDto
    {
        public Guid Id { get; set; }
        public DateTime ScheduledStart { get; set; }
        public DateTime ScheduledEnd { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public string ProviderName { get; set; } = string.Empty;
    }

    private sealed class PatientVisitDto
    {
        public Guid Id { get; set; }
        public DateTime VisitStart { get; set; }
        public DateTime? VisitEnd { get; set; }
        public string VisitType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Summary { get; set; }
    }

    private sealed class DashboardDto
    {
        public Guid ClinicId { get; set; }
        public string ClinicName { get; set; } = string.Empty;
        public int PatientCount { get; set; }
        public int StaffCount { get; set; }
        public int ProviderCount { get; set; }
        public int FacilityCount { get; set; }
        public int AppointmentsTodayCount { get; set; }
        public int UpcomingAppointmentsCount { get; set; }
        public List<AppointmentListItemDto>? UpcomingAppointments { get; set; }
    }

    private sealed class ProviderListItemDto
    {
        public Guid ProviderId { get; set; }
        public Guid ApplicationUserId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int ScheduleSlotCount { get; set; }
    }

    private sealed class ProviderDetailDto
    {
        public Guid ProviderId { get; set; }
        public Guid ApplicationUserId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public List<ProviderScheduleDto>? Schedules { get; set; }
    }

    private sealed class ProviderScheduleDto
    {
        public Guid Id { get; set; }
        public DayOfWeek Day { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsRecurring { get; set; }
    }

    private sealed class PagedAppointmentsDto
    {
        public List<AppointmentListItemDto>? Items { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    private sealed class AppointmentListItemDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public Guid ProviderId { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public DateTime ScheduledStart { get; set; }
        public DateTime ScheduledEnd { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Reason { get; set; }
    }
}
