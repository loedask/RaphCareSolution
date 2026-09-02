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
                    ReferenceCode = c.ReferenceCode,
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
            if (string.IsNullOrWhiteSpace(registrationNumber))
                return Response<Guid?>.Failure("Enter a hospital reference or registration number.", 400);

            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PostAsJsonAsync("api/admin/clinics/claim", new { registrationNumber }, cancellationToken)
                .ConfigureAwait(false);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return Response<Guid?>.Failure("No hospital found with that reference or registration number, or it is already linked to another team.", 404);

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
                    ReferenceCode = dto.ReferenceCode,
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
        string? jobRole = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PostAsJsonAsync($"api/admin/clinics/{clinicId}/staff", new { email, jobRole }, cancellationToken)
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
        string? jobRole = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PutAsJsonAsync($"api/admin/clinics/{clinicId}/staff/{userId}/role", new { isAdministrator, jobRole }, cancellationToken)
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

    public async Task<Response<bool>> SetProviderActiveAsync(
        Guid clinicId,
        Guid providerId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PostAsJsonAsync(
                    $"api/admin/clinics/{clinicId}/providers/{providerId}/active",
                    new { isActive },
                    cancellationToken)
                .ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
                return Response<bool>.Success(true);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return Response<bool>.Failure("Provider not found.", 404);

            var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                error = "Only hospital administrators can change provider status.";
            return Response<bool>.Failure(error, (int)response.StatusCode);
        }
        catch (HttpRequestException)
        {
            return Response<bool>.Failure("We couldn't reach the server. Check your connection and try again.");
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

    public async Task<Response<ClinicVisitDetail>> StartVisitAsync(
        Guid clinicId,
        Guid appointmentId,
        string? summary = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PostAsJsonAsync(
                    $"api/admin/clinics/{clinicId}/appointments/{appointmentId}/visit",
                    new { summary },
                    cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    error = "Only hospital administrators can start visits.";
                return Response<ClinicVisitDetail>.Failure(error, (int)response.StatusCode);
            }

            var dto = await response.Content.ReadFromJsonAsync<VisitDetailDto>(JsonOptions, cancellationToken).ConfigureAwait(false);
            if (dto is null)
                return Response<ClinicVisitDetail>.Failure("Could not start visit.");

            return Response<ClinicVisitDetail>.Success(MapVisit(dto));
        }
        catch (HttpRequestException)
        {
            return Response<ClinicVisitDetail>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<ClinicVisitDetail>> GetVisitAsync(
        Guid clinicId,
        Guid visitId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .GetAsync($"api/admin/clinics/{clinicId}/visits/{visitId}", cancellationToken)
                .ConfigureAwait(false);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return Response<ClinicVisitDetail>.Failure("Visit not found.", 404);
            if (!response.IsSuccessStatusCode)
                return Response<ClinicVisitDetail>.Failure(await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false), (int)response.StatusCode);

            var dto = await response.Content.ReadFromJsonAsync<VisitDetailDto>(JsonOptions, cancellationToken).ConfigureAwait(false);
            if (dto is null)
                return Response<ClinicVisitDetail>.Failure("Could not load visit.");

            return Response<ClinicVisitDetail>.Success(MapVisit(dto));
        }
        catch (HttpRequestException)
        {
            return Response<ClinicVisitDetail>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<ClinicVisitDetail>> CompleteVisitAsync(
        Guid clinicId,
        Guid visitId,
        string? summary = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PostAsJsonAsync(
                    $"api/admin/clinics/{clinicId}/visits/{visitId}/complete",
                    new { summary },
                    cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    error = "Only hospital administrators can complete visits.";
                return Response<ClinicVisitDetail>.Failure(error, (int)response.StatusCode);
            }

            var dto = await response.Content.ReadFromJsonAsync<VisitDetailDto>(JsonOptions, cancellationToken).ConfigureAwait(false);
            if (dto is null)
                return Response<ClinicVisitDetail>.Failure("Could not complete visit.");

            return Response<ClinicVisitDetail>.Success(MapVisit(dto));
        }
        catch (HttpRequestException)
        {
            return Response<ClinicVisitDetail>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<ClinicVisitVital>> RecordVisitVitalAsync(
        Guid clinicId,
        Guid visitId,
        RecordVisitVitalRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PostAsJsonAsync($"api/admin/clinics/{clinicId}/visits/{visitId}/vitals", request, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    error = "Only hospital administrators can record vitals.";
                return Response<ClinicVisitVital>.Failure(error, (int)response.StatusCode);
            }

            var dto = await response.Content.ReadFromJsonAsync<VisitVitalDto>(JsonOptions, cancellationToken).ConfigureAwait(false);
            if (dto is null)
                return Response<ClinicVisitVital>.Failure("Could not record vital.");

            return Response<ClinicVisitVital>.Success(new ClinicVisitVital
            {
                Id = dto.Id,
                Type = dto.Type,
                Value = dto.Value,
                Unit = dto.Unit,
                RecordedAt = dto.RecordedAt
            });
        }
        catch (HttpRequestException)
        {
            return Response<ClinicVisitVital>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<ClinicVisitSoapNote>> SaveVisitSoapNoteAsync(
        Guid clinicId,
        Guid visitId,
        SaveVisitSoapNoteRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PutAsJsonAsync($"api/admin/clinics/{clinicId}/visits/{visitId}/soap", request, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    error = "Only hospital administrators can save visit notes.";
                return Response<ClinicVisitSoapNote>.Failure(error, (int)response.StatusCode);
            }

            var dto = await response.Content.ReadFromJsonAsync<VisitSoapNoteDto>(JsonOptions, cancellationToken).ConfigureAwait(false);
            if (dto is null)
                return Response<ClinicVisitSoapNote>.Failure("Could not save the SOAP note.");

            return Response<ClinicVisitSoapNote>.Success(MapSoapNote(dto));
        }
        catch (HttpRequestException)
        {
            return Response<ClinicVisitSoapNote>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<ClinicVisitNote>> AddVisitNoteAsync(
        Guid clinicId,
        Guid visitId,
        AddVisitNoteRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PostAsJsonAsync($"api/admin/clinics/{clinicId}/visits/{visitId}/notes", request, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    error = "Only hospital administrators can add visit notes.";
                return Response<ClinicVisitNote>.Failure(error, (int)response.StatusCode);
            }

            var dto = await response.Content.ReadFromJsonAsync<VisitNoteDto>(JsonOptions, cancellationToken).ConfigureAwait(false);
            if (dto is null)
                return Response<ClinicVisitNote>.Failure("Could not add the note.");

            return Response<ClinicVisitNote>.Success(MapClinicalNote(dto));
        }
        catch (HttpRequestException)
        {
            return Response<ClinicVisitNote>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<ClinicVisitPrescription>> AddVisitPrescriptionAsync(
        Guid clinicId,
        Guid visitId,
        AddVisitPrescriptionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PostAsJsonAsync($"api/admin/clinics/{clinicId}/visits/{visitId}/prescriptions", request, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    error = "Only hospital administrators can add prescriptions.";
                return Response<ClinicVisitPrescription>.Failure(error, (int)response.StatusCode);
            }

            var dto = await response.Content.ReadFromJsonAsync<VisitPrescriptionDto>(JsonOptions, cancellationToken).ConfigureAwait(false);
            if (dto is null)
                return Response<ClinicVisitPrescription>.Failure("Could not add the prescription.");

            return Response<ClinicVisitPrescription>.Success(MapPrescription(dto));
        }
        catch (HttpRequestException)
        {
            return Response<ClinicVisitPrescription>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<ClinicVisitLabResult>> AddVisitLabResultAsync(
        Guid clinicId,
        Guid visitId,
        AddVisitLabResultRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PostAsJsonAsync($"api/admin/clinics/{clinicId}/visits/{visitId}/lab-orders", request, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    error = "Only hospital administrators can order lab tests.";
                return Response<ClinicVisitLabResult>.Failure(error, (int)response.StatusCode);
            }

            var dto = await response.Content.ReadFromJsonAsync<VisitLabResultDto>(JsonOptions, cancellationToken).ConfigureAwait(false);
            if (dto is null)
                return Response<ClinicVisitLabResult>.Failure("Could not order the lab test.");

            return Response<ClinicVisitLabResult>.Success(MapLabResult(dto));
        }
        catch (HttpRequestException)
        {
            return Response<ClinicVisitLabResult>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<ClinicVisitLabResult>> CompleteLabOrderAsync(
        Guid clinicId,
        Guid labRequestId,
        CompleteVisitLabOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PostAsJsonAsync($"api/admin/clinics/{clinicId}/lab-orders/{labRequestId}/results", request, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    error = "Only hospital staff can record lab results.";
                return Response<ClinicVisitLabResult>.Failure(error, (int)response.StatusCode);
            }

            var dto = await response.Content.ReadFromJsonAsync<VisitLabResultDto>(JsonOptions, cancellationToken).ConfigureAwait(false);
            if (dto is null)
                return Response<ClinicVisitLabResult>.Failure("Could not record the lab result.");

            return Response<ClinicVisitLabResult>.Success(MapLabResult(dto));
        }
        catch (HttpRequestException)
        {
            return Response<ClinicVisitLabResult>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<ClinicVisitPrescription>> DispensePrescriptionAsync(
        Guid clinicId,
        Guid prescriptionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PostAsync($"api/admin/clinics/{clinicId}/prescriptions/{prescriptionId}/dispense", null, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    error = "Only hospital staff can mark a prescription as collected.";
                return Response<ClinicVisitPrescription>.Failure(error, (int)response.StatusCode);
            }

            var dto = await response.Content.ReadFromJsonAsync<VisitPrescriptionDto>(JsonOptions, cancellationToken).ConfigureAwait(false);
            if (dto is null)
                return Response<ClinicVisitPrescription>.Failure("Could not mark the prescription as collected.");

            return Response<ClinicVisitPrescription>.Success(MapPrescription(dto));
        }
        catch (HttpRequestException)
        {
            return Response<ClinicVisitPrescription>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public Task<Response<ClinicVisitPrescription>> CancelPrescriptionAsync(
        Guid clinicId,
        Guid prescriptionId,
        CancellationToken cancellationToken = default) =>
        PostPrescriptionActionAsync(
            clinicId,
            prescriptionId,
            "cancel",
            "Only hospital staff can cancel a prescription.",
            "Could not cancel the prescription.",
            cancellationToken);

    public Task<Response<ClinicVisitPrescription>> UndoPrescriptionAsync(
        Guid clinicId,
        Guid prescriptionId,
        CancellationToken cancellationToken = default) =>
        PostPrescriptionActionAsync(
            clinicId,
            prescriptionId,
            "undo",
            "Only hospital staff can undo a prescription collection.",
            "Could not undo the prescription.",
            cancellationToken);

    public Task<Response<ClinicVisitLabResult>> CancelLabOrderAsync(
        Guid clinicId,
        Guid labRequestId,
        CancellationToken cancellationToken = default) =>
        PostLabActionAsync(
            clinicId,
            labRequestId,
            "cancel",
            "Only hospital staff can cancel a lab order.",
            "Could not cancel the lab order.",
            cancellationToken);

    public Task<Response<ClinicVisitLabResult>> UndoLabOrderAsync(
        Guid clinicId,
        Guid labRequestId,
        CancellationToken cancellationToken = default) =>
        PostLabActionAsync(
            clinicId,
            labRequestId,
            "undo",
            "Only hospital staff can undo a lab result.",
            "Could not undo the lab result.",
            cancellationToken);

    public Task<Response<ClinicVisitPrescription>> CallPrescriptionAsync(
        Guid clinicId,
        Guid prescriptionId,
        CancellationToken cancellationToken = default) =>
        PostPrescriptionActionAsync(
            clinicId,
            prescriptionId,
            "call",
            "Only hospital staff can call a pickup code.",
            "Could not call this pickup code.",
            cancellationToken);

    public Task<Response<ClinicVisitLabResult>> CallLabOrderAsync(
        Guid clinicId,
        Guid labRequestId,
        CancellationToken cancellationToken = default) =>
        PostLabActionAsync(
            clinicId,
            labRequestId,
            "call",
            "Only hospital staff can call a pickup code.",
            "Could not call this pickup code.",
            cancellationToken);

    public async Task<Response<string>> EnsureCollectionDisplayTokenAsync(
        Guid clinicId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PostAsync($"api/admin/clinics/{clinicId}/collection-display", null, cancellationToken)
                .ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return Response<string>.Failure(await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false), (int)response.StatusCode);

            var dto = await response.Content.ReadFromJsonAsync<CollectionDisplayLinkDto>(JsonOptions, cancellationToken).ConfigureAwait(false);
            if (dto is null || string.IsNullOrWhiteSpace(dto.Token))
                return Response<string>.Failure("Could not open the waiting screen.");

            return Response<string>.Success(dto.Token);
        }
        catch (HttpRequestException)
        {
            return Response<string>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<ClinicCasualtyBoard>> GetCasualtyBoardAsync(
        Guid clinicId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .GetAsync($"api/admin/clinics/{clinicId}/casualty/board", cancellationToken)
                .ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return Response<ClinicCasualtyBoard>.Failure("Hospital not found or you do not have access.", 404);
            if (!response.IsSuccessStatusCode)
                return Response<ClinicCasualtyBoard>.Failure(await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false), (int)response.StatusCode);

            var dto = await response.Content.ReadFromJsonAsync<CasualtyBoardDto>(JsonOptions, cancellationToken)
                .ConfigureAwait(false);
            if (dto is null)
                return Response<ClinicCasualtyBoard>.Failure("Could not load the casualty board.");

            return Response<ClinicCasualtyBoard>.Success(MapCasualtyBoard(dto));
        }
        catch (HttpRequestException)
        {
            return Response<ClinicCasualtyBoard>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<ClinicCasualtyTicket>> CreateCasualtyTicketAsync(
        Guid clinicId,
        CreateCasualtyTicketRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PostAsJsonAsync(
                    $"api/admin/clinics/{clinicId}/casualty/tickets",
                    new
                    {
                        request.PatientId,
                        request.TriageLevel,
                        request.ChiefComplaint
                    },
                    JsonOptions,
                    cancellationToken)
                .ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return Response<ClinicCasualtyTicket>.Failure(await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false), (int)response.StatusCode);

            var dto = await response.Content.ReadFromJsonAsync<CasualtyTicketDto>(JsonOptions, cancellationToken)
                .ConfigureAwait(false);
            if (dto is null)
                return Response<ClinicCasualtyTicket>.Failure("Could not add the casualty ticket.");

            return Response<ClinicCasualtyTicket>.Success(MapCasualtyTicket(dto));
        }
        catch (HttpRequestException)
        {
            return Response<ClinicCasualtyTicket>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<ClinicCasualtyTicket>> CallCasualtyTicketAsync(
        Guid clinicId,
        Guid ticketId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PostAsync($"api/admin/clinics/{clinicId}/casualty/tickets/{ticketId}/call", null, cancellationToken)
                .ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return Response<ClinicCasualtyTicket>.Failure(await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false), (int)response.StatusCode);

            var dto = await response.Content.ReadFromJsonAsync<CasualtyTicketDto>(JsonOptions, cancellationToken)
                .ConfigureAwait(false);
            if (dto is null)
                return Response<ClinicCasualtyTicket>.Failure("Could not call this queue code.");

            return Response<ClinicCasualtyTicket>.Success(MapCasualtyTicket(dto));
        }
        catch (HttpRequestException)
        {
            return Response<ClinicCasualtyTicket>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<ClinicCasualtyTicket>> CompleteCasualtyTicketAsync(
        Guid clinicId,
        Guid ticketId,
        bool cancel = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PostAsJsonAsync(
                    $"api/admin/clinics/{clinicId}/casualty/tickets/{ticketId}/complete",
                    new { Cancel = cancel },
                    JsonOptions,
                    cancellationToken)
                .ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return Response<ClinicCasualtyTicket>.Failure(await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false), (int)response.StatusCode);

            var dto = await response.Content.ReadFromJsonAsync<CasualtyTicketDto>(JsonOptions, cancellationToken)
                .ConfigureAwait(false);
            if (dto is null)
                return Response<ClinicCasualtyTicket>.Failure("Could not close this casualty ticket.");

            return Response<ClinicCasualtyTicket>.Success(MapCasualtyTicket(dto));
        }
        catch (HttpRequestException)
        {
            return Response<ClinicCasualtyTicket>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<string>> EnsureCasualtyDisplayTokenAsync(
        Guid clinicId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PostAsync($"api/admin/clinics/{clinicId}/casualty-display", null, cancellationToken)
                .ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return Response<string>.Failure(await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false), (int)response.StatusCode);

            var dto = await response.Content.ReadFromJsonAsync<CollectionDisplayLinkDto>(JsonOptions, cancellationToken)
                .ConfigureAwait(false);
            if (dto is null || string.IsNullOrWhiteSpace(dto.Token))
                return Response<string>.Failure("Could not open the casualty waiting screen.");

            return Response<string>.Success(dto.Token);
        }
        catch (HttpRequestException)
        {
            return Response<string>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<ClinicTheatreBoard>> GetTheatreBoardAsync(
        Guid clinicId,
        DateTime? dayUtc = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            var url = $"api/admin/clinics/{clinicId}/theatre/board";
            if (dayUtc is not null)
                url += $"?dayUtc={Uri.EscapeDataString(dayUtc.Value.ToUniversalTime().ToString("O"))}";

            using var response = await client.GetAsync(url, cancellationToken).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return Response<ClinicTheatreBoard>.Failure("Hospital not found or you do not have access.", 404);
            if (!response.IsSuccessStatusCode)
                return Response<ClinicTheatreBoard>.Failure(await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false), (int)response.StatusCode);

            var dto = await response.Content.ReadFromJsonAsync<TheatreBoardDto>(JsonOptions, cancellationToken)
                .ConfigureAwait(false);
            if (dto is null)
                return Response<ClinicTheatreBoard>.Failure("Could not load the theatre board.");

            return Response<ClinicTheatreBoard>.Success(MapTheatreBoard(dto));
        }
        catch (HttpRequestException)
        {
            return Response<ClinicTheatreBoard>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<ClinicTheatreCase>> CreateTheatreCaseAsync(
        Guid clinicId,
        CreateTheatreCaseRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PostAsJsonAsync(
                    $"api/admin/clinics/{clinicId}/theatre/cases",
                    new
                    {
                        request.PatientId,
                        request.ScheduledStart,
                        request.ScheduledEnd,
                        request.ProcedureName,
                        request.TheatreName,
                        request.SurgeonName,
                        request.Notes
                    },
                    JsonOptions,
                    cancellationToken)
                .ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return Response<ClinicTheatreCase>.Failure(await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false), (int)response.StatusCode);

            var dto = await response.Content.ReadFromJsonAsync<TheatreCaseDto>(JsonOptions, cancellationToken)
                .ConfigureAwait(false);
            if (dto is null)
                return Response<ClinicTheatreCase>.Failure("Could not schedule the theatre case.");

            return Response<ClinicTheatreCase>.Success(MapTheatreCase(dto));
        }
        catch (HttpRequestException)
        {
            return Response<ClinicTheatreCase>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<ClinicTheatreCase>> UpdateTheatreCaseStatusAsync(
        Guid clinicId,
        Guid caseId,
        string status,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PostAsJsonAsync(
                    $"api/admin/clinics/{clinicId}/theatre/cases/{caseId}/status",
                    new { Status = status },
                    JsonOptions,
                    cancellationToken)
                .ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return Response<ClinicTheatreCase>.Failure(await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false), (int)response.StatusCode);

            var dto = await response.Content.ReadFromJsonAsync<TheatreCaseDto>(JsonOptions, cancellationToken)
                .ConfigureAwait(false);
            if (dto is null)
                return Response<ClinicTheatreCase>.Failure("Could not update the theatre case.");

            return Response<ClinicTheatreCase>.Success(MapTheatreCase(dto));
        }
        catch (HttpRequestException)
        {
            return Response<ClinicTheatreCase>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<ClinicReferralBoard>> GetReferralBoardAsync(
        Guid clinicId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .GetAsync($"api/admin/clinics/{clinicId}/referrals/board", cancellationToken)
                .ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return Response<ClinicReferralBoard>.Failure("Hospital not found or you do not have access.", 404);
            if (!response.IsSuccessStatusCode)
                return Response<ClinicReferralBoard>.Failure(await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false), (int)response.StatusCode);

            var dto = await response.Content.ReadFromJsonAsync<ReferralBoardDto>(JsonOptions, cancellationToken)
                .ConfigureAwait(false);
            if (dto is null)
                return Response<ClinicReferralBoard>.Failure("Could not load the referral board.");

            return Response<ClinicReferralBoard>.Success(MapReferralBoard(dto));
        }
        catch (HttpRequestException)
        {
            return Response<ClinicReferralBoard>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<ClinicReferral>> CreateReferralAsync(
        Guid clinicId,
        CreateReferralRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PostAsJsonAsync(
                    $"api/admin/clinics/{clinicId}/referrals",
                    new
                    {
                        request.PatientId,
                        request.VisitId,
                        request.ReferredTo,
                        request.Reason,
                        request.Specialty,
                        request.Notes
                    },
                    JsonOptions,
                    cancellationToken)
                .ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return Response<ClinicReferral>.Failure(await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false), (int)response.StatusCode);

            var dto = await response.Content.ReadFromJsonAsync<ReferralDto>(JsonOptions, cancellationToken)
                .ConfigureAwait(false);
            if (dto is null)
                return Response<ClinicReferral>.Failure("Could not log the referral.");

            return Response<ClinicReferral>.Success(MapReferral(dto));
        }
        catch (HttpRequestException)
        {
            return Response<ClinicReferral>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<ClinicReferral>> UpdateReferralStatusAsync(
        Guid clinicId,
        Guid referralId,
        string status,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PostAsJsonAsync(
                    $"api/admin/clinics/{clinicId}/referrals/{referralId}/status",
                    new { Status = status },
                    JsonOptions,
                    cancellationToken)
                .ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return Response<ClinicReferral>.Failure(await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false), (int)response.StatusCode);

            var dto = await response.Content.ReadFromJsonAsync<ReferralDto>(JsonOptions, cancellationToken)
                .ConfigureAwait(false);
            if (dto is null)
                return Response<ClinicReferral>.Failure("Could not update the referral.");

            return Response<ClinicReferral>.Success(MapReferral(dto));
        }
        catch (HttpRequestException)
        {
            return Response<ClinicReferral>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    private static ClinicCasualtyBoard MapCasualtyBoard(CasualtyBoardDto dto) => new()
    {
        ClinicName = dto.ClinicName ?? string.Empty,
        WaitingCount = dto.WaitingCount,
        CalledCount = dto.CalledCount,
        Waiting = dto.Waiting?.Select(MapCasualtyTicket).ToList() ?? [],
        Called = dto.Called?.Select(MapCasualtyTicket).ToList() ?? [],
        Recent = dto.Recent?.Select(MapCasualtyTicket).ToList() ?? []
    };

    private static ClinicCasualtyTicket MapCasualtyTicket(CasualtyTicketDto dto) => new()
    {
        Id = dto.Id,
        PatientId = dto.PatientId,
        PatientName = dto.PatientName,
        QueueCode = dto.QueueCode ?? string.Empty,
        TriageLevel = dto.TriageLevel ?? string.Empty,
        ChiefComplaint = dto.ChiefComplaint,
        Status = dto.Status ?? string.Empty,
        ArrivedAt = dto.ArrivedAt,
        CalledAt = dto.CalledAt,
        CompletedAt = dto.CompletedAt
    };

    private static ClinicTheatreBoard MapTheatreBoard(TheatreBoardDto dto) => new()
    {
        ClinicName = dto.ClinicName ?? string.Empty,
        DayUtc = dto.DayUtc,
        ScheduledCount = dto.ScheduledCount,
        InProgressCount = dto.InProgressCount,
        CompletedCount = dto.CompletedCount,
        Cases = dto.Cases?.Select(MapTheatreCase).ToList() ?? []
    };

    private static ClinicTheatreCase MapTheatreCase(TheatreCaseDto dto) => new()
    {
        Id = dto.Id,
        PatientId = dto.PatientId,
        PatientName = dto.PatientName ?? string.Empty,
        ScheduledStart = dto.ScheduledStart,
        ScheduledEnd = dto.ScheduledEnd,
        ProcedureName = dto.ProcedureName ?? string.Empty,
        TheatreName = dto.TheatreName,
        SurgeonName = dto.SurgeonName,
        Status = dto.Status ?? string.Empty,
        Notes = dto.Notes
    };

    private static ClinicReferralBoard MapReferralBoard(ReferralBoardDto dto) => new()
    {
        ClinicName = dto.ClinicName ?? string.Empty,
        SentCount = dto.SentCount,
        AcceptedCount = dto.AcceptedCount,
        CompletedCount = dto.CompletedCount,
        Open = dto.Open?.Select(MapReferral).ToList() ?? [],
        Recent = dto.Recent?.Select(MapReferral).ToList() ?? []
    };

    private static ClinicReferral MapReferral(ReferralDto dto) => new()
    {
        Id = dto.Id,
        PatientId = dto.PatientId,
        PatientName = dto.PatientName ?? string.Empty,
        VisitId = dto.VisitId,
        ReferredTo = dto.ReferredTo ?? string.Empty,
        Reason = dto.Reason,
        Specialty = dto.Specialty,
        Notes = dto.Notes,
        Status = dto.Status ?? string.Empty,
        ReferredAt = dto.ReferredAt,
        AcceptedAt = dto.AcceptedAt,
        CompletedAt = dto.CompletedAt
    };

    private async Task<Response<ClinicVisitPrescription>> PostPrescriptionActionAsync(
        Guid clinicId,
        Guid prescriptionId,
        string action,
        string forbiddenMessage,
        string fallbackMessage,
        CancellationToken cancellationToken)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PostAsync($"api/admin/clinics/{clinicId}/prescriptions/{prescriptionId}/{action}", null, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    error = forbiddenMessage;
                return Response<ClinicVisitPrescription>.Failure(error, (int)response.StatusCode);
            }

            var dto = await response.Content.ReadFromJsonAsync<VisitPrescriptionDto>(JsonOptions, cancellationToken).ConfigureAwait(false);
            if (dto is null)
                return Response<ClinicVisitPrescription>.Failure(fallbackMessage);

            return Response<ClinicVisitPrescription>.Success(MapPrescription(dto));
        }
        catch (HttpRequestException)
        {
            return Response<ClinicVisitPrescription>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    private async Task<Response<ClinicVisitLabResult>> PostLabActionAsync(
        Guid clinicId,
        Guid labRequestId,
        string action,
        string forbiddenMessage,
        string fallbackMessage,
        CancellationToken cancellationToken)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PostAsync($"api/admin/clinics/{clinicId}/lab-orders/{labRequestId}/{action}", null, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    error = forbiddenMessage;
                return Response<ClinicVisitLabResult>.Failure(error, (int)response.StatusCode);
            }

            var dto = await response.Content.ReadFromJsonAsync<VisitLabResultDto>(JsonOptions, cancellationToken).ConfigureAwait(false);
            if (dto is null)
                return Response<ClinicVisitLabResult>.Failure(fallbackMessage);

            return Response<ClinicVisitLabResult>.Success(MapLabResult(dto));
        }
        catch (HttpRequestException)
        {
            return Response<ClinicVisitLabResult>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<ClinicCollectionBoard>> GetCollectionOrdersAsync(
        Guid clinicId,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            var url = $"api/admin/clinics/{clinicId}/collection-orders";
            if (!string.IsNullOrWhiteSpace(search))
                url += $"?search={Uri.EscapeDataString(search.Trim())}";

            using var response = await client.GetAsync(url, cancellationToken).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return Response<ClinicCollectionBoard>.Failure("Hospital not found or you do not have access.", 404);
            if (!response.IsSuccessStatusCode)
                return Response<ClinicCollectionBoard>.Failure(await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false), (int)response.StatusCode);

            var dto = await response.Content.ReadFromJsonAsync<CollectionBoardDto>(JsonOptions, cancellationToken).ConfigureAwait(false);
            if (dto is null)
                return Response<ClinicCollectionBoard>.Failure("Could not load collection orders.");

            return Response<ClinicCollectionBoard>.Success(new ClinicCollectionBoard
            {
                Prescriptions = MapCollectionPrescriptions(dto.Prescriptions),
                LabOrders = MapCollectionLabs(dto.LabOrders),
                RecentPrescriptions = MapCollectionPrescriptions(dto.RecentPrescriptions),
                RecentLabOrders = MapCollectionLabs(dto.RecentLabOrders)
            });
        }
        catch (HttpRequestException)
        {
            return Response<ClinicCollectionBoard>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<IReadOnlyList<ClinicDeviceListItem>>> GetDevicesAsync(
        Guid clinicId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .GetAsync($"api/admin/clinics/{clinicId}/devices", cancellationToken)
                .ConfigureAwait(false);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return Response<IReadOnlyList<ClinicDeviceListItem>>.Failure("Hospital not found or you do not have access.", 404);
            if (!response.IsSuccessStatusCode)
                return Response<IReadOnlyList<ClinicDeviceListItem>>.Failure(await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false), (int)response.StatusCode);

            var items = await response.Content.ReadFromJsonAsync<List<DeviceListItemDto>>(JsonOptions, cancellationToken).ConfigureAwait(false);
            return Response<IReadOnlyList<ClinicDeviceListItem>>.Success(
                items?.Select(d => new ClinicDeviceListItem
                {
                    Id = d.Id,
                    SerialNumber = d.SerialNumber,
                    Model = d.Model,
                    IsActive = d.IsActive,
                    IsAssigned = d.IsAssigned,
                    Status = d.Status,
                    AssignedPatientId = d.AssignedPatientId,
                    AssignedPatientName = d.AssignedPatientName
                }).ToList() ?? []);
        }
        catch (HttpRequestException)
        {
            return Response<IReadOnlyList<ClinicDeviceListItem>>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<ClinicInpatientBoard>> GetInpatientBoardAsync(
        Guid clinicId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .GetAsync($"api/admin/clinics/{clinicId}/inpatient/board", cancellationToken)
                .ConfigureAwait(false);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return Response<ClinicInpatientBoard>.Failure("Hospital not found or you do not have access.", 404);
            if (!response.IsSuccessStatusCode)
                return Response<ClinicInpatientBoard>.Failure(await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false), (int)response.StatusCode);

            var dto = await response.Content.ReadFromJsonAsync<InpatientBoardDto>(JsonOptions, cancellationToken).ConfigureAwait(false);
            if (dto is null)
                return Response<ClinicInpatientBoard>.Failure("Could not load inpatient board.");

            return Response<ClinicInpatientBoard>.Success(MapInpatientBoard(dto));
        }
        catch (HttpRequestException)
        {
            return Response<ClinicInpatientBoard>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<ClinicWard>> CreateWardAsync(
        Guid clinicId,
        CreateWardRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PostAsJsonAsync($"api/admin/clinics/{clinicId}/wards", request, cancellationToken)
                .ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    error = "Only hospital administrators can create wards.";
                return Response<ClinicWard>.Failure(error, (int)response.StatusCode);
            }
            var dto = await response.Content.ReadFromJsonAsync<WardDto>(JsonOptions, cancellationToken).ConfigureAwait(false);
            if (dto is null) return Response<ClinicWard>.Failure("Could not create ward.");
            return Response<ClinicWard>.Success(MapWard(dto));
        }
        catch (HttpRequestException)
        {
            return Response<ClinicWard>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<ClinicWard>> UpdateWardAsync(
        Guid clinicId,
        Guid wardId,
        UpdateWardRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PutAsJsonAsync($"api/admin/clinics/{clinicId}/wards/{wardId}", request, cancellationToken)
                .ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    error = "Only hospital administrators can update wards.";
                return Response<ClinicWard>.Failure(error, (int)response.StatusCode);
            }
            var dto = await response.Content.ReadFromJsonAsync<WardDto>(JsonOptions, cancellationToken).ConfigureAwait(false);
            if (dto is null) return Response<ClinicWard>.Failure("Could not update ward.");
            return Response<ClinicWard>.Success(MapWard(dto));
        }
        catch (HttpRequestException)
        {
            return Response<ClinicWard>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<bool>> DeleteWardAsync(
        Guid clinicId,
        Guid wardId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .DeleteAsync($"api/admin/clinics/{clinicId}/wards/{wardId}", cancellationToken)
                .ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return Response<bool>.Failure("Ward not found.", 404);
            if (!response.IsSuccessStatusCode)
            {
                var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    error = "Only hospital administrators can delete wards.";
                return Response<bool>.Failure(error, (int)response.StatusCode);
            }
            return Response<bool>.Success(true);
        }
        catch (HttpRequestException)
        {
            return Response<bool>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<ClinicRoom>> CreateRoomAsync(
        Guid clinicId,
        CreateRoomRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PostAsJsonAsync($"api/admin/clinics/{clinicId}/rooms", request, cancellationToken)
                .ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    error = "Only hospital administrators can create rooms.";
                return Response<ClinicRoom>.Failure(error, (int)response.StatusCode);
            }
            var dto = await response.Content.ReadFromJsonAsync<RoomDto>(JsonOptions, cancellationToken).ConfigureAwait(false);
            if (dto is null) return Response<ClinicRoom>.Failure("Could not create room.");
            return Response<ClinicRoom>.Success(MapRoom(dto));
        }
        catch (HttpRequestException)
        {
            return Response<ClinicRoom>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<ClinicRoom>> UpdateRoomAsync(
        Guid clinicId,
        Guid roomId,
        UpdateRoomRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PutAsJsonAsync($"api/admin/clinics/{clinicId}/rooms/{roomId}", request, cancellationToken)
                .ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    error = "Only hospital administrators can update rooms.";
                return Response<ClinicRoom>.Failure(error, (int)response.StatusCode);
            }
            var dto = await response.Content.ReadFromJsonAsync<RoomDto>(JsonOptions, cancellationToken).ConfigureAwait(false);
            if (dto is null) return Response<ClinicRoom>.Failure("Could not update room.");
            return Response<ClinicRoom>.Success(MapRoom(dto));
        }
        catch (HttpRequestException)
        {
            return Response<ClinicRoom>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<bool>> DeleteRoomAsync(
        Guid clinicId,
        Guid roomId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .DeleteAsync($"api/admin/clinics/{clinicId}/rooms/{roomId}", cancellationToken)
                .ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return Response<bool>.Failure("Room not found.", 404);
            if (!response.IsSuccessStatusCode)
            {
                var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    error = "Only hospital administrators can delete rooms.";
                return Response<bool>.Failure(error, (int)response.StatusCode);
            }
            return Response<bool>.Success(true);
        }
        catch (HttpRequestException)
        {
            return Response<bool>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<ClinicBed>> CreateBedAsync(
        Guid clinicId,
        CreateBedRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PostAsJsonAsync($"api/admin/clinics/{clinicId}/beds", request, cancellationToken)
                .ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    error = "Only hospital administrators can create beds.";
                return Response<ClinicBed>.Failure(error, (int)response.StatusCode);
            }
            var dto = await response.Content.ReadFromJsonAsync<BedDto>(JsonOptions, cancellationToken).ConfigureAwait(false);
            if (dto is null) return Response<ClinicBed>.Failure("Could not create bed.");
            return Response<ClinicBed>.Success(MapBed(dto));
        }
        catch (HttpRequestException)
        {
            return Response<ClinicBed>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<ClinicBed>> UpdateBedAsync(
        Guid clinicId,
        Guid bedId,
        UpdateBedRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PutAsJsonAsync($"api/admin/clinics/{clinicId}/beds/{bedId}", request, cancellationToken)
                .ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    error = "Only hospital administrators can update beds.";
                return Response<ClinicBed>.Failure(error, (int)response.StatusCode);
            }
            var dto = await response.Content.ReadFromJsonAsync<BedDto>(JsonOptions, cancellationToken).ConfigureAwait(false);
            if (dto is null) return Response<ClinicBed>.Failure("Could not update bed.");
            return Response<ClinicBed>.Success(MapBed(dto));
        }
        catch (HttpRequestException)
        {
            return Response<ClinicBed>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<ClinicBed>> SetBedStatusAsync(
        Guid clinicId,
        Guid bedId,
        string status,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PutAsJsonAsync(
                    $"api/admin/clinics/{clinicId}/beds/{bedId}/status",
                    new SetBedStatusRequest { Status = status },
                    cancellationToken)
                .ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    error = "Only hospital administrators can change bed status.";
                return Response<ClinicBed>.Failure(error, (int)response.StatusCode);
            }
            var dto = await response.Content.ReadFromJsonAsync<BedDto>(JsonOptions, cancellationToken).ConfigureAwait(false);
            if (dto is null) return Response<ClinicBed>.Failure("Could not update bed status.");
            return Response<ClinicBed>.Success(MapBed(dto));
        }
        catch (HttpRequestException)
        {
            return Response<ClinicBed>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<bool>> DeleteBedAsync(
        Guid clinicId,
        Guid bedId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .DeleteAsync($"api/admin/clinics/{clinicId}/beds/{bedId}", cancellationToken)
                .ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return Response<bool>.Failure("Bed not found.", 404);
            if (!response.IsSuccessStatusCode)
            {
                var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    error = "Only hospital administrators can delete beds.";
                return Response<bool>.Failure(error, (int)response.StatusCode);
            }
            return Response<bool>.Success(true);
        }
        catch (HttpRequestException)
        {
            return Response<bool>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<ClinicAdmission>> AdmitPatientAsync(
        Guid clinicId,
        AdmitPatientRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PostAsJsonAsync($"api/admin/clinics/{clinicId}/admissions", request, cancellationToken)
                .ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    error = "Only hospital administrators can admit patients.";
                return Response<ClinicAdmission>.Failure(error, (int)response.StatusCode);
            }
            var dto = await response.Content.ReadFromJsonAsync<AdmissionDto>(JsonOptions, cancellationToken).ConfigureAwait(false);
            if (dto is null) return Response<ClinicAdmission>.Failure("Could not admit patient.");
            return Response<ClinicAdmission>.Success(MapAdmission(dto));
        }
        catch (HttpRequestException)
        {
            return Response<ClinicAdmission>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<ClinicAdmission>> DischargeAdmissionAsync(
        Guid clinicId,
        Guid admissionId,
        DischargeAdmissionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PostAsJsonAsync(
                    $"api/admin/clinics/{clinicId}/admissions/{admissionId}/discharge",
                    request,
                    cancellationToken)
                .ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    error = "Only hospital administrators can discharge patients.";
                return Response<ClinicAdmission>.Failure(error, (int)response.StatusCode);
            }
            var dto = await response.Content.ReadFromJsonAsync<AdmissionDto>(JsonOptions, cancellationToken).ConfigureAwait(false);
            if (dto is null) return Response<ClinicAdmission>.Failure("Could not discharge patient.");
            return Response<ClinicAdmission>.Success(MapAdmission(dto));
        }
        catch (HttpRequestException)
        {
            return Response<ClinicAdmission>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<IReadOnlyList<ClinicAdmissionObservation>>> GetAdmissionObservationsAsync(
        Guid clinicId,
        Guid admissionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .GetAsync($"api/admin/clinics/{clinicId}/admissions/{admissionId}/observations", cancellationToken)
                .ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return Response<IReadOnlyList<ClinicAdmissionObservation>>.Failure("Admission not found.", 404);
            if (!response.IsSuccessStatusCode)
                return Response<IReadOnlyList<ClinicAdmissionObservation>>.Failure(
                    await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false),
                    (int)response.StatusCode);

            var items = await response.Content
                .ReadFromJsonAsync<List<AdmissionObservationDto>>(JsonOptions, cancellationToken)
                .ConfigureAwait(false);
            IReadOnlyList<ClinicAdmissionObservation> observations =
                items?.Select(MapObservation).ToList() ?? [];
            return Response<IReadOnlyList<ClinicAdmissionObservation>>.Success(observations);
        }
        catch (HttpRequestException)
        {
            return Response<IReadOnlyList<ClinicAdmissionObservation>>.Failure(
                "We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<ClinicAdmissionObservation>> AddAdmissionObservationAsync(
        Guid clinicId,
        Guid admissionId,
        AddAdmissionObservationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PostAsJsonAsync(
                    $"api/admin/clinics/{clinicId}/admissions/{admissionId}/observations",
                    request,
                    cancellationToken)
                .ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    error = "Only a nurse, doctor, or hospital administrator can add a ward note.";
                return Response<ClinicAdmissionObservation>.Failure(error, (int)response.StatusCode);
            }

            var dto = await response.Content
                .ReadFromJsonAsync<AdmissionObservationDto>(JsonOptions, cancellationToken)
                .ConfigureAwait(false);
            if (dto is null)
                return Response<ClinicAdmissionObservation>.Failure("Could not save the ward note.");
            return Response<ClinicAdmissionObservation>.Success(MapObservation(dto));
        }
        catch (HttpRequestException)
        {
            return Response<ClinicAdmissionObservation>.Failure(
                "We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<ClinicAdmission>> TransferAdmissionAsync(
        Guid clinicId,
        Guid admissionId,
        TransferAdmissionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PostAsJsonAsync(
                    $"api/admin/clinics/{clinicId}/admissions/{admissionId}/transfer",
                    request,
                    cancellationToken)
                .ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    error = "Only hospital administrators can transfer patients.";
                return Response<ClinicAdmission>.Failure(error, (int)response.StatusCode);
            }
            var dto = await response.Content.ReadFromJsonAsync<AdmissionDto>(JsonOptions, cancellationToken).ConfigureAwait(false);
            if (dto is null) return Response<ClinicAdmission>.Failure("Could not transfer patient.");
            return Response<ClinicAdmission>.Success(MapAdmission(dto));
        }
        catch (HttpRequestException)
        {
            return Response<ClinicAdmission>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<PagedClinicAdmissions>> GetAdmissionsAsync(
        Guid clinicId,
        int pageNumber = 1,
        int pageSize = 20,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            var query = $"api/admin/clinics/{clinicId}/admissions?pageNumber={pageNumber}&pageSize={pageSize}";
            if (!string.IsNullOrWhiteSpace(status))
                query += $"&status={Uri.EscapeDataString(status.Trim())}";

            using var response = await client.GetAsync(query, cancellationToken).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return Response<PagedClinicAdmissions>.Failure("Hospital not found or you do not have access.", 404);
            if (!response.IsSuccessStatusCode)
                return Response<PagedClinicAdmissions>.Failure(await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false), (int)response.StatusCode);

            var page = await response.Content.ReadFromJsonAsync<PagedAdmissionsDto>(JsonOptions, cancellationToken).ConfigureAwait(false);
            if (page?.Items is null)
                return Response<PagedClinicAdmissions>.Failure("Could not load admissions.");

            return Response<PagedClinicAdmissions>.Success(new PagedClinicAdmissions
            {
                Items = page.Items.Select(MapAdmission).ToList(),
                TotalCount = page.TotalCount,
                PageNumber = page.PageNumber,
                PageSize = page.PageSize
            });
        }
        catch (HttpRequestException)
        {
            return Response<PagedClinicAdmissions>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<ClinicAdmission>> GetAdmissionByIdAsync(
        Guid clinicId,
        Guid admissionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .GetAsync($"api/admin/clinics/{clinicId}/admissions/{admissionId}", cancellationToken)
                .ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return Response<ClinicAdmission>.Failure("Admission not found.", 404);
            if (!response.IsSuccessStatusCode)
                return Response<ClinicAdmission>.Failure(await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false), (int)response.StatusCode);

            var dto = await response.Content.ReadFromJsonAsync<AdmissionDto>(JsonOptions, cancellationToken).ConfigureAwait(false);
            if (dto is null) return Response<ClinicAdmission>.Failure("Could not load admission.");
            return Response<ClinicAdmission>.Success(MapAdmission(dto));
        }
        catch (HttpRequestException)
        {
            return Response<ClinicAdmission>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    public async Task<Response<ClinicTeleJoinInfo>> StartTeleSessionAsync(
        Guid clinicId,
        Guid appointmentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient(ServiceRegistration.HttpClientName);
            using var response = await client
                .PostAsync($"api/admin/clinics/{clinicId}/appointments/{appointmentId}/tele-session", null, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var error = await ReadErrorAsync(response, cancellationToken).ConfigureAwait(false);
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    error = "Only hospital administrators can start telehealth sessions.";
                return Response<ClinicTeleJoinInfo>.Failure(error, (int)response.StatusCode);
            }

            var dto = await response.Content.ReadFromJsonAsync<TeleJoinInfoDto>(JsonOptions, cancellationToken).ConfigureAwait(false);
            if (dto is null)
                return Response<ClinicTeleJoinInfo>.Failure("Could not start telehealth session.");

            return Response<ClinicTeleJoinInfo>.Success(new ClinicTeleJoinInfo
            {
                TeleSessionId = dto.TeleSessionId,
                VisitId = dto.VisitId,
                AppointmentId = dto.AppointmentId,
                ChannelName = dto.ChannelName,
                Uid = dto.Uid,
                AppId = dto.AppId,
                RtcToken = dto.RtcToken,
                TokenExpiresAtUnix = dto.TokenExpiresAtUnix,
                RtcConfigured = dto.RtcConfigured,
                Status = dto.Status,
                ScheduledStart = dto.ScheduledStart,
                PatientName = dto.PatientName,
                ProviderDisplayName = dto.ProviderDisplayName
            });
        }
        catch (HttpRequestException)
        {
            return Response<ClinicTeleJoinInfo>.Failure("We couldn't reach the server. Check your connection and try again.");
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
        ReferenceCode = dto.ReferenceCode,
        Country = dto.Country,
        TimeZone = dto.TimeZone,
        IsActive = dto.IsActive,
        CreatedAt = dto.CreatedAt,
        RegisteredByApplicationUserId = dto.RegisteredByApplicationUserId,
        CurrentUserIsAdministrator = dto.CurrentUserIsAdministrator,
        CurrentUserCanDocumentVisits = dto.CurrentUserCanDocumentVisits,
        CurrentUserCanRecordWardNotes = dto.CurrentUserCanRecordWardNotes,
        CurrentUserCanDispense = dto.CurrentUserCanDispense,
        CurrentUserCanCompleteLabs = dto.CurrentUserCanCompleteLabs,
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
        NationalHealthId = dto.NationalHealthId,
        AccessType = dto.AccessType,
        GrantedAt = dto.GrantedAt,
        GrantedByRule = dto.GrantedByRule,
        Notes = dto.Notes,
        MedicalSummary = MapMedicalSummary(dto.MedicalSummary),
        EmergencyContacts = dto.EmergencyContacts?.Select(MapEmergencyContact).ToList() ?? [],
        InsuranceProfiles = dto.InsuranceProfiles?.Select(MapInsurance).ToList() ?? [],
        Invoices = dto.Invoices?.Select(MapInvoice).ToList() ?? [],
        MoodLogs = dto.MoodLogs?.Select(MapMoodLog).ToList() ?? [],
        CarePlans = dto.CarePlans?.Select(MapCarePlan).ToList() ?? [],
        Diagnoses = dto.Diagnoses?.Select(MapDiagnosis).ToList() ?? [],
        Prescriptions = dto.Prescriptions?.Select(MapPrescription).ToList() ?? [],
        ClinicalNotes = dto.ClinicalNotes?.Select(MapClinicalNote).ToList() ?? [],
        SoapNotes = dto.SoapNotes?.Select(MapSoapNote).ToList() ?? [],
        LabResults = dto.LabResults?.Select(MapLabResult).ToList() ?? [],
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

    private static ClinicPatientMedicalSummary MapMedicalSummary(PatientMedicalSummaryDto? dto) => dto is null
        ? new ClinicPatientMedicalSummary()
        : new ClinicPatientMedicalSummary
        {
            BloodType = dto.BloodType,
            Allergies = dto.Allergies,
            ChronicConditions = dto.ChronicConditions,
            Medications = dto.Medications,
            PrimaryDoctor = dto.PrimaryDoctor,
            RecordedAllergies = dto.RecordedAllergies?.Select(a => new ClinicPatientAllergy
            {
                Substance = a.Substance,
                Reaction = a.Reaction,
                Severity = a.Severity,
                RecordedAt = a.RecordedAt
            }).ToList() ?? [],
            RecordedMedications = dto.RecordedMedications?.Select(m => new ClinicPatientMedication
            {
                MedicationName = m.MedicationName,
                Dosage = m.Dosage,
                Frequency = m.Frequency,
                StartDate = m.StartDate,
                EndDate = m.EndDate
            }).ToList() ?? []
        };

    private static ClinicPatientEmergencyContact MapEmergencyContact(PatientEmergencyContactDto dto) => new()
    {
        Name = dto.Name,
        Relationship = dto.Relationship,
        PhoneNumber = dto.PhoneNumber,
        Email = dto.Email
    };

    private static ClinicPatientInsuranceProfile MapInsurance(PatientInsuranceDto dto) => new()
    {
        PlanName = dto.PlanName,
        MembershipNumber = dto.MembershipNumber,
        StartDate = dto.StartDate,
        EndDate = dto.EndDate,
        IsActive = dto.IsActive
    };

    private static ClinicPatientInvoice MapInvoice(PatientInvoiceDto dto) => new()
    {
        Id = dto.Id,
        Amount = dto.Amount,
        Currency = dto.Currency,
        Status = dto.Status,
        DueDate = dto.DueDate,
        PaidAt = dto.PaidAt,
        VisitId = dto.VisitId
    };

    private static ClinicPatientMoodLog MapMoodLog(PatientMoodLogDto dto) => new()
    {
        LoggedAt = dto.LoggedAt,
        MoodScore = dto.MoodScore,
        Notes = dto.Notes,
        IsFlagged = dto.IsFlagged
    };

    private static ClinicPatientCarePlan MapCarePlan(PatientCarePlanDto dto) => new()
    {
        Title = dto.Title,
        Description = dto.Description,
        StartDate = dto.StartDate,
        EndDate = dto.EndDate,
        Status = dto.Status
    };

    private static ClinicVisitDiagnosis MapDiagnosis(VisitDiagnosisDto dto) => new()
    {
        VisitId = dto.VisitId,
        VisitStart = dto.VisitStart,
        Code = dto.Code,
        Description = dto.Description,
        Severity = dto.Severity
    };

    private static ClinicVisitPrescription MapPrescription(VisitPrescriptionDto dto) => new()
    {
        Id = dto.Id,
        VisitId = dto.VisitId,
        VisitStart = dto.VisitStart,
        IssuedAt = dto.IssuedAt,
        Status = dto.Status ?? "Pending",
        PickupCode = dto.PickupCode ?? string.Empty,
        DispensedAt = dto.DispensedAt,
        Notes = dto.Notes,
        Items = dto.Items?.Select(i => new ClinicVisitPrescriptionItem
        {
            MedicationName = i.MedicationName,
            Dosage = i.Dosage,
            Frequency = i.Frequency,
            DurationDays = i.DurationDays
        }).ToList() ?? []
    };

    private static List<ClinicCollectionPrescription> MapCollectionPrescriptions(
        List<CollectionPrescriptionDto>? items) =>
        items?.Select(p => new ClinicCollectionPrescription
        {
            Id = p.Id,
            VisitId = p.VisitId,
            PatientId = p.PatientId,
            PatientName = p.PatientName ?? "Patient",
            NationalHealthId = p.NationalHealthId,
            PickupCode = p.PickupCode ?? string.Empty,
            IssuedAt = p.IssuedAt,
            Status = p.Status ?? "Pending",
            DispensedAt = p.DispensedAt,
            CalledAt = p.CalledAt,
            Notes = p.Notes,
            Items = p.Items?.Select(i => new ClinicVisitPrescriptionItem
            {
                MedicationName = i.MedicationName ?? string.Empty,
                Dosage = i.Dosage,
                Frequency = i.Frequency,
                DurationDays = i.DurationDays
            }).ToList() ?? []
        }).ToList() ?? [];

    private static List<ClinicCollectionLabOrder> MapCollectionLabs(
        List<CollectionLabOrderDto>? items) =>
        items?.Select(l => new ClinicCollectionLabOrder
        {
            Id = l.Id,
            VisitId = l.VisitId,
            PatientId = l.PatientId,
            PatientName = l.PatientName ?? "Patient",
            NationalHealthId = l.NationalHealthId,
            PickupCode = l.PickupCode ?? string.Empty,
            TestName = l.TestName ?? string.Empty,
            Status = l.Status ?? "Pending",
            RequestedAt = l.RequestedAt,
            CalledAt = l.CalledAt
        }).ToList() ?? [];

    private static ClinicVisitNote MapClinicalNote(VisitNoteDto dto) => new()
    {
        VisitId = dto.VisitId,
        VisitStart = dto.VisitStart,
        Notes = dto.Notes,
        Category = dto.Category
    };

    private static ClinicVisitSoapNote MapSoapNote(VisitSoapNoteDto dto) => new()
    {
        VisitId = dto.VisitId,
        VisitStart = dto.VisitStart,
        Subjective = dto.Subjective,
        Objective = dto.Objective,
        Assessment = dto.Assessment,
        Plan = dto.Plan
    };

    private static ClinicVisitLabResult MapLabResult(VisitLabResultDto dto) => new()
    {
        Id = dto.Id,
        VisitId = dto.VisitId,
        VisitStart = dto.VisitStart,
        TestName = dto.TestName,
        Status = dto.Status ?? "Pending",
        PickupCode = dto.PickupCode ?? string.Empty,
        RequestedAt = dto.RequestedAt,
        ResultValue = dto.ResultValue,
        Unit = dto.Unit,
        ReferenceRange = dto.ReferenceRange,
        ReportedAt = dto.ReportedAt
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
        TotalBeds = dto.TotalBeds,
        OccupiedBeds = dto.OccupiedBeds,
        OccupancyPercent = dto.OccupancyPercent,
        AdmissionsTodayCount = dto.AdmissionsTodayCount,
        DischargesTodayCount = dto.DischargesTodayCount,
        AverageLengthOfStayDays = dto.AverageLengthOfStayDays,
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
        Reason = dto.Reason,
        ActiveVisitId = dto.ActiveVisitId
    };

    private static ClinicVisitDetail MapVisit(VisitDetailDto dto) => new()
    {
        Id = dto.Id,
        ClinicId = dto.ClinicId,
        AppointmentId = dto.AppointmentId,
        PatientId = dto.PatientId,
        PatientName = dto.PatientName,
        ProviderId = dto.ProviderId,
        ProviderName = dto.ProviderName,
        VisitStart = dto.VisitStart,
        VisitEnd = dto.VisitEnd,
        VisitType = dto.VisitType,
        Status = dto.Status,
        Summary = dto.Summary,
        Vitals = dto.Vitals?.Select(v => new ClinicVisitVital
        {
            Id = v.Id,
            Type = v.Type,
            Value = v.Value,
            Unit = v.Unit,
            RecordedAt = v.RecordedAt
        }).ToList() ?? [],
        Diagnoses = dto.Diagnoses?.Select(MapDiagnosis).ToList() ?? [],
        Prescriptions = dto.Prescriptions?.Select(MapPrescription).ToList() ?? [],
        ClinicalNotes = dto.ClinicalNotes?.Select(MapClinicalNote).ToList() ?? [],
        SoapNotes = dto.SoapNotes?.Select(MapSoapNote).ToList() ?? [],
        LabResults = dto.LabResults?.Select(MapLabResult).ToList() ?? []
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
        public string ReferenceCode { get; set; } = string.Empty;
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
        public string ReferenceCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string TimeZone { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? RegisteredByApplicationUserId { get; set; }
        public bool CurrentUserIsAdministrator { get; set; }
        public bool CurrentUserCanDocumentVisits { get; set; }
        public bool CurrentUserCanRecordWardNotes { get; set; }
        public bool CurrentUserCanDispense { get; set; }
        public bool CurrentUserCanCompleteLabs { get; set; }
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
        public string ReferenceCode { get; set; } = string.Empty;
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
        public string? NationalHealthId { get; set; }
        public string AccessType { get; set; } = string.Empty;
        public DateTime GrantedAt { get; set; }
        public string GrantedByRule { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public PatientMedicalSummaryDto? MedicalSummary { get; set; }
        public List<PatientEmergencyContactDto>? EmergencyContacts { get; set; }
        public List<PatientInsuranceDto>? InsuranceProfiles { get; set; }
        public List<PatientInvoiceDto>? Invoices { get; set; }
        public List<PatientMoodLogDto>? MoodLogs { get; set; }
        public List<PatientCarePlanDto>? CarePlans { get; set; }
        public List<VisitDiagnosisDto>? Diagnoses { get; set; }
        public List<VisitPrescriptionDto>? Prescriptions { get; set; }
        public List<VisitNoteDto>? ClinicalNotes { get; set; }
        public List<VisitSoapNoteDto>? SoapNotes { get; set; }
        public List<VisitLabResultDto>? LabResults { get; set; }
        public List<PatientVisitDto>? RecentVisits { get; set; }
        public List<PatientAppointmentDto>? Appointments { get; set; }
        public List<PatientVitalDto>? RecentVitals { get; set; }
        public List<PatientDeviceReadingDto>? RecentDeviceReadings { get; set; }
        public List<PatientDeviceRollupDto>? DeviceDailyRollups { get; set; }
    }

    private sealed class PatientVitalDto
    {
        public string Type { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string? Unit { get; set; }
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
        public int TotalBeds { get; set; }
        public int OccupiedBeds { get; set; }
        public int OccupancyPercent { get; set; }
        public int AdmissionsTodayCount { get; set; }
        public int DischargesTodayCount { get; set; }
        public decimal? AverageLengthOfStayDays { get; set; }
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
        public Guid? ActiveVisitId { get; set; }
    }

    private sealed class VisitDetailDto
    {
        public Guid Id { get; set; }
        public Guid ClinicId { get; set; }
        public Guid AppointmentId { get; set; }
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public Guid ProviderId { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public DateTime VisitStart { get; set; }
        public DateTime? VisitEnd { get; set; }
        public string VisitType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public List<VisitVitalDto>? Vitals { get; set; }
        public List<VisitDiagnosisDto>? Diagnoses { get; set; }
        public List<VisitPrescriptionDto>? Prescriptions { get; set; }
        public List<VisitNoteDto>? ClinicalNotes { get; set; }
        public List<VisitSoapNoteDto>? SoapNotes { get; set; }
        public List<VisitLabResultDto>? LabResults { get; set; }
    }

    private sealed class VisitVitalDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string? Unit { get; set; }
        public DateTime RecordedAt { get; set; }
    }

    private sealed class PatientMedicalSummaryDto
    {
        public string? BloodType { get; set; }
        public string? Allergies { get; set; }
        public string? ChronicConditions { get; set; }
        public string? Medications { get; set; }
        public string? PrimaryDoctor { get; set; }
        public List<PatientAllergyDto>? RecordedAllergies { get; set; }
        public List<PatientMedicationDto>? RecordedMedications { get; set; }
    }

    private sealed class PatientAllergyDto
    {
        public string Substance { get; set; } = string.Empty;
        public string? Reaction { get; set; }
        public string? Severity { get; set; }
        public DateTime RecordedAt { get; set; }
    }

    private sealed class PatientMedicationDto
    {
        public string MedicationName { get; set; } = string.Empty;
        public string? Dosage { get; set; }
        public string? Frequency { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    private sealed class PatientEmergencyContactDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Relationship { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
    }

    private sealed class PatientInsuranceDto
    {
        public string PlanName { get; set; } = string.Empty;
        public string MembershipNumber { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
    }

    private sealed class PatientInvoiceDto
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public DateTime? PaidAt { get; set; }
        public Guid? VisitId { get; set; }
    }

    private sealed class PatientMoodLogDto
    {
        public DateTime LoggedAt { get; set; }
        public int MoodScore { get; set; }
        public string? Notes { get; set; }
        public bool IsFlagged { get; set; }
    }

    private sealed class PatientCarePlanDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    private sealed class VisitDiagnosisDto
    {
        public Guid VisitId { get; set; }
        public DateTime VisitStart { get; set; }
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Severity { get; set; }
    }

    private sealed class VisitPrescriptionDto
    {
        public Guid Id { get; set; }
        public Guid VisitId { get; set; }
        public DateTime VisitStart { get; set; }
        public DateTime IssuedAt { get; set; }
        public string? Status { get; set; }
        public string? PickupCode { get; set; }
        public DateTime? DispensedAt { get; set; }
        public string? Notes { get; set; }
        public List<VisitPrescriptionItemDto>? Items { get; set; }
    }

    private sealed class VisitPrescriptionItemDto
    {
        public string MedicationName { get; set; } = string.Empty;
        public string? Dosage { get; set; }
        public string? Frequency { get; set; }
        public int DurationDays { get; set; }
    }

    private sealed class VisitNoteDto
    {
        public Guid VisitId { get; set; }
        public DateTime VisitStart { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string? Category { get; set; }
    }

    private sealed class VisitSoapNoteDto
    {
        public Guid VisitId { get; set; }
        public DateTime VisitStart { get; set; }
        public string? Subjective { get; set; }
        public string? Objective { get; set; }
        public string? Assessment { get; set; }
        public string? Plan { get; set; }
    }

    private sealed class VisitLabResultDto
    {
        public Guid Id { get; set; }
        public Guid VisitId { get; set; }
        public DateTime VisitStart { get; set; }
        public string TestName { get; set; } = string.Empty;
        public string? Status { get; set; }
        public string? PickupCode { get; set; }
        public DateTime RequestedAt { get; set; }
        public string? ResultValue { get; set; }
        public string? Unit { get; set; }
        public string? ReferenceRange { get; set; }
        public DateTime? ReportedAt { get; set; }
    }

    private sealed class CollectionDisplayLinkDto
    {
        public string Token { get; set; } = string.Empty;
    }

    private sealed class CasualtyBoardDto
    {
        public string? ClinicName { get; set; }
        public int WaitingCount { get; set; }
        public int CalledCount { get; set; }
        public List<CasualtyTicketDto>? Waiting { get; set; }
        public List<CasualtyTicketDto>? Called { get; set; }
        public List<CasualtyTicketDto>? Recent { get; set; }
    }

    private sealed class CasualtyTicketDto
    {
        public Guid Id { get; set; }
        public Guid? PatientId { get; set; }
        public string? PatientName { get; set; }
        public string? QueueCode { get; set; }
        public string? TriageLevel { get; set; }
        public string? ChiefComplaint { get; set; }
        public string? Status { get; set; }
        public DateTime ArrivedAt { get; set; }
        public DateTime? CalledAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    private sealed class TheatreBoardDto
    {
        public string? ClinicName { get; set; }
        public DateTime DayUtc { get; set; }
        public int ScheduledCount { get; set; }
        public int InProgressCount { get; set; }
        public int CompletedCount { get; set; }
        public List<TheatreCaseDto>? Cases { get; set; }
    }

    private sealed class TheatreCaseDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string? PatientName { get; set; }
        public DateTime ScheduledStart { get; set; }
        public DateTime? ScheduledEnd { get; set; }
        public string? ProcedureName { get; set; }
        public string? TheatreName { get; set; }
        public string? SurgeonName { get; set; }
        public string? Status { get; set; }
        public string? Notes { get; set; }
    }

    private sealed class ReferralBoardDto
    {
        public string? ClinicName { get; set; }
        public int SentCount { get; set; }
        public int AcceptedCount { get; set; }
        public int CompletedCount { get; set; }
        public List<ReferralDto>? Open { get; set; }
        public List<ReferralDto>? Recent { get; set; }
    }

    private sealed class ReferralDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string? PatientName { get; set; }
        public Guid? VisitId { get; set; }
        public string? ReferredTo { get; set; }
        public string? Reason { get; set; }
        public string? Specialty { get; set; }
        public string? Notes { get; set; }
        public string? Status { get; set; }
        public DateTime ReferredAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    private sealed class CollectionBoardDto
    {
        public List<CollectionPrescriptionDto>? Prescriptions { get; set; }
        public List<CollectionLabOrderDto>? LabOrders { get; set; }
        public List<CollectionPrescriptionDto>? RecentPrescriptions { get; set; }
        public List<CollectionLabOrderDto>? RecentLabOrders { get; set; }
    }

    private sealed class CollectionPrescriptionDto
    {
        public Guid Id { get; set; }
        public Guid VisitId { get; set; }
        public Guid PatientId { get; set; }
        public string? PatientName { get; set; }
        public string? NationalHealthId { get; set; }
        public string? PickupCode { get; set; }
        public DateTime IssuedAt { get; set; }
        public string? Status { get; set; }
        public DateTime? DispensedAt { get; set; }
        public DateTime? CalledAt { get; set; }
        public string? Notes { get; set; }
        public List<VisitPrescriptionItemDto>? Items { get; set; }
    }

    private sealed class CollectionLabOrderDto
    {
        public Guid Id { get; set; }
        public Guid VisitId { get; set; }
        public Guid PatientId { get; set; }
        public string? PatientName { get; set; }
        public string? NationalHealthId { get; set; }
        public string? PickupCode { get; set; }
        public string? TestName { get; set; }
        public string? Status { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? CalledAt { get; set; }
    }

    private sealed class DeviceListItemDto
    {
        public Guid Id { get; set; }
        public string SerialNumber { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsAssigned { get; set; }
        public string Status { get; set; } = string.Empty;
        public Guid? AssignedPatientId { get; set; }
        public string? AssignedPatientName { get; set; }
    }

    private sealed class TeleJoinInfoDto
    {
        public Guid TeleSessionId { get; set; }
        public Guid VisitId { get; set; }
        public Guid AppointmentId { get; set; }
        public string ChannelName { get; set; } = string.Empty;
        public uint Uid { get; set; }
        public string? AppId { get; set; }
        public string? RtcToken { get; set; }
        public long TokenExpiresAtUnix { get; set; }
        public bool RtcConfigured { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime ScheduledStart { get; set; }
        public string? PatientName { get; set; }
        public string? ProviderDisplayName { get; set; }
    }

    private static ClinicInpatientBoard MapInpatientBoard(InpatientBoardDto dto) => new()
    {
        ClinicId = dto.ClinicId,
        TotalBeds = dto.TotalBeds,
        AvailableBeds = dto.AvailableBeds,
        OccupiedBeds = dto.OccupiedBeds,
        MaintenanceBeds = dto.MaintenanceBeds,
        ActiveAdmissions = dto.ActiveAdmissions,
        OccupancyPercent = dto.OccupancyPercent,
        AdmissionsTodayCount = dto.AdmissionsTodayCount,
        DischargesTodayCount = dto.DischargesTodayCount,
        AverageLengthOfStayDays = dto.AverageLengthOfStayDays,
        Wards = dto.Wards?.Select(MapWard).ToList() ?? [],
        ActiveAdmissionsList = dto.ActiveAdmissionsList?.Select(MapAdmission).ToList() ?? []
    };

    private static ClinicWard MapWard(WardDto dto) => new()
    {
        Id = dto.Id,
        FacilityId = dto.FacilityId,
        FacilityName = dto.FacilityName,
        Name = dto.Name,
        Code = dto.Code,
        IsActive = dto.IsActive,
        Rooms = dto.Rooms?.Select(MapRoom).ToList() ?? []
    };

    private static ClinicRoom MapRoom(RoomDto dto) => new()
    {
        Id = dto.Id,
        WardId = dto.WardId,
        Name = dto.Name,
        RoomType = dto.RoomType,
        IsActive = dto.IsActive,
        Beds = dto.Beds?.Select(MapBed).ToList() ?? []
    };

    private static ClinicBed MapBed(BedDto dto) => new()
    {
        Id = dto.Id,
        RoomId = dto.RoomId,
        Label = dto.Label,
        Status = dto.Status,
        IsActive = dto.IsActive,
        CurrentAdmissionId = dto.CurrentAdmissionId,
        CurrentPatientId = dto.CurrentPatientId,
        CurrentPatientName = dto.CurrentPatientName
    };

    private static ClinicAdmission MapAdmission(AdmissionDto dto) => new()
    {
        Id = dto.Id,
        PatientId = dto.PatientId,
        PatientName = dto.PatientName,
        BedId = dto.BedId,
        BedLabel = dto.BedLabel,
        RoomName = dto.RoomName,
        WardName = dto.WardName,
        FacilityName = dto.FacilityName,
        AdmittedAt = dto.AdmittedAt,
        DischargedAt = dto.DischargedAt,
        Status = dto.Status,
        Reason = dto.Reason,
        Notes = dto.Notes,
        DischargeSummary = dto.DischargeSummary,
        InvoiceId = dto.InvoiceId,
        InvoiceAmount = dto.InvoiceAmount,
        InvoiceCurrency = dto.InvoiceCurrency,
        InvoiceStatus = dto.InvoiceStatus,
        InvoicePaidAt = dto.InvoicePaidAt,
        BedNights = dto.BedNights
    };

    private static ClinicAdmissionObservation MapObservation(AdmissionObservationDto dto) => new()
    {
        Id = dto.Id,
        AdmissionId = dto.AdmissionId,
        RecordedAt = dto.RecordedAt,
        Note = dto.Note,
        HeartRate = dto.HeartRate,
        TemperatureCelsius = dto.TemperatureCelsius,
        OxygenSaturation = dto.OxygenSaturation,
        SystolicBp = dto.SystolicBp,
        DiastolicBp = dto.DiastolicBp
    };

    private sealed class PagedAdmissionsDto
    {
        public List<AdmissionDto>? Items { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    private sealed class InpatientBoardDto
    {
        public Guid ClinicId { get; set; }
        public int TotalBeds { get; set; }
        public int AvailableBeds { get; set; }
        public int OccupiedBeds { get; set; }
        public int MaintenanceBeds { get; set; }
        public int ActiveAdmissions { get; set; }
        public int OccupancyPercent { get; set; }
        public int AdmissionsTodayCount { get; set; }
        public int DischargesTodayCount { get; set; }
        public decimal? AverageLengthOfStayDays { get; set; }
        public List<WardDto>? Wards { get; set; }
        public List<AdmissionDto>? ActiveAdmissionsList { get; set; }
    }

    private sealed class WardDto
    {
        public Guid Id { get; set; }
        public Guid FacilityId { get; set; }
        public string FacilityName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public bool IsActive { get; set; }
        public List<RoomDto>? Rooms { get; set; }
    }

    private sealed class RoomDto
    {
        public Guid Id { get; set; }
        public Guid WardId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? RoomType { get; set; }
        public bool IsActive { get; set; }
        public List<BedDto>? Beds { get; set; }
    }

    private sealed class BedDto
    {
        public Guid Id { get; set; }
        public Guid RoomId { get; set; }
        public string Label { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public Guid? CurrentAdmissionId { get; set; }
        public Guid? CurrentPatientId { get; set; }
        public string? CurrentPatientName { get; set; }
    }

    private sealed class AdmissionDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public Guid BedId { get; set; }
        public string BedLabel { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public string WardName { get; set; } = string.Empty;
        public string FacilityName { get; set; } = string.Empty;
        public DateTime AdmittedAt { get; set; }
        public DateTime? DischargedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public string? Notes { get; set; }
        public string? DischargeSummary { get; set; }
        public Guid? InvoiceId { get; set; }
        public decimal? InvoiceAmount { get; set; }
        public string? InvoiceCurrency { get; set; }
        public string? InvoiceStatus { get; set; }
        public DateTime? InvoicePaidAt { get; set; }
        public int? BedNights { get; set; }
    }

    private sealed class AdmissionObservationDto
    {
        public Guid Id { get; set; }
        public Guid AdmissionId { get; set; }
        public DateTime RecordedAt { get; set; }
        public string Note { get; set; } = string.Empty;
        public decimal? HeartRate { get; set; }
        public decimal? TemperatureCelsius { get; set; }
        public decimal? OxygenSaturation { get; set; }
        public decimal? SystolicBp { get; set; }
        public decimal? DiastolicBp { get; set; }
    }
}
