using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Api;
using RaphCare.Client.Models.Appointments;
using RaphCare.Client.Models.Insurance;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client.Services;

public sealed class PatientInsuranceService(HttpClient httpClient) : BaseHttpService(httpClient), IPatientInsuranceService
{
    public async Task<Response<IReadOnlyList<InsurancePlanOptionViewModel>>> GetActivePlansAsync(CancellationToken cancellationToken = default)
    {
        var result = await GetAsync<IReadOnlyList<InsurancePlanOptionDto>>("api/patient/insurance/plans", cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess)
            return Response<IReadOnlyList<InsurancePlanOptionViewModel>>.Failure(result.ErrorMessage ?? "Could not load plans.", result.StatusCode);

        var list = (result.Data ?? Array.Empty<InsurancePlanOptionDto>())
            .Select(d => new InsurancePlanOptionViewModel
            {
                Id = d.Id,
                Name = d.Name ?? string.Empty,
                Code = d.Code ?? string.Empty
            })
            .ToList();
        return Response<IReadOnlyList<InsurancePlanOptionViewModel>>.Success(list);
    }

    public async Task<Response<PagedPatientInsuranceProfilesViewModel>> GetMyProfilesAsync(
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await GetAsync<PagedApiResult<InsuranceProfileDto>>(
                $"api/patient/insurance/profiles?pageNumber={pageNumber}&pageSize={pageSize}",
                cancellationToken)
            .ConfigureAwait(false);

        if (!result.IsSuccess || result.Data is null)
            return Response<PagedPatientInsuranceProfilesViewModel>.Failure(result.ErrorMessage ?? "Could not load profiles.", result.StatusCode);

        return Response<PagedPatientInsuranceProfilesViewModel>.Success(MapPaged(result.Data));
    }

    public async Task<Response<PatientInsuranceProfileViewModel?>> GetMyProfileAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await GetAsync<InsuranceProfileDto>($"api/patient/insurance/profiles/{id}", cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess)
            return Response<PatientInsuranceProfileViewModel?>.Failure(result.ErrorMessage ?? "Could not load profile.", result.StatusCode);
        return Response<PatientInsuranceProfileViewModel?>.Success(result.Data is null ? null : MapProfile(result.Data));
    }

    public async Task<Response<Guid>> CreateProfileAsync(CreatePatientInsuranceProfileRequest request, CancellationToken cancellationToken = default)
    {
        var result = await PostAsync<CreatedGuidApiResponse>("api/patient/insurance/profiles", request, cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess || result.Data is null)
            return Response<Guid>.Failure(result.ErrorMessage ?? "Create failed.", result.StatusCode);
        return Response<Guid>.Success(result.Data.Id);
    }

    public async Task<Response<bool>> UpdateProfileAsync(UpdatePatientInsuranceProfileRequest request, CancellationToken cancellationToken = default)
    {
        var result = await PutNoContentAsync($"api/patient/insurance/profiles/{request.Id}", request, cancellationToken).ConfigureAwait(false);
        return result.IsSuccess
            ? Response<bool>.Success(true)
            : Response<bool>.Failure(result.ErrorMessage ?? "Update failed.", result.StatusCode);
    }

    private static PagedPatientInsuranceProfilesViewModel MapPaged(PagedApiResult<InsuranceProfileDto> paged) =>
        new()
        {
            Items = (paged.Items ?? Array.Empty<InsuranceProfileDto>()).Select(MapProfile).ToList(),
            TotalCount = paged.TotalCount,
            PageNumber = paged.PageNumber,
            PageSize = paged.PageSize
        };

    private static PatientInsuranceProfileViewModel MapProfile(InsuranceProfileDto d) =>
        new()
        {
            Id = d.Id,
            InsurancePlanId = d.InsurancePlanId,
            PlanName = d.PlanName ?? string.Empty,
            PlanCode = d.PlanCode ?? string.Empty,
            MembershipNumber = d.MembershipNumber ?? string.Empty,
            StartDate = d.StartDate,
            EndDate = d.EndDate,
            IsActive = d.IsActive
        };

    private sealed class InsurancePlanOptionDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
    }

    private sealed class InsuranceProfileDto
    {
        public Guid Id { get; set; }
        public Guid InsurancePlanId { get; set; }
        public string? PlanName { get; set; }
        public string? PlanCode { get; set; }
        public string? MembershipNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
    }
}
