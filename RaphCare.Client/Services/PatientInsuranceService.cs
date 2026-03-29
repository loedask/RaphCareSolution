using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Appointments;
using RaphCare.Client.Models.Insurance;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client.Services;

public class PatientInsuranceService(IClient client, HttpClient httpClient) : BaseHttpService(client, httpClient), IPatientInsuranceService
{
    public async Task<Response<IReadOnlyList<InsurancePlanOptionViewModel>>> GetActivePlansAsync(CancellationToken cancellationToken = default)
    {
        var r = await GetAsync<List<InsurancePlanOptionViewModel>>("api/patient/insurance/plans", cancellationToken).ConfigureAwait(false);
        if (!r.IsSuccess || r.Data is null)
            return Response<IReadOnlyList<InsurancePlanOptionViewModel>>.Failure(r.ErrorMessage ?? "Request failed.", r.StatusCode);
        return Response<IReadOnlyList<InsurancePlanOptionViewModel>>.Success(r.Data);
    }

    public Task<Response<PagedPatientInsuranceProfilesViewModel>> GetMyProfilesAsync(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default) =>
        GetAsync<PagedPatientInsuranceProfilesViewModel>($"api/patient/insurance/profiles?pageNumber={pageNumber}&pageSize={pageSize}", cancellationToken);

    public Task<Response<PatientInsuranceProfileViewModel?>> GetMyProfileAsync(Guid id, CancellationToken cancellationToken = default) =>
        GetAsync<PatientInsuranceProfileViewModel?>($"api/patient/insurance/profiles/{id}", cancellationToken);

    public async Task<Response<Guid>> CreateProfileAsync(CreatePatientInsuranceProfileRequest request, CancellationToken cancellationToken = default)
    {
        var result = await PostAsync<CreatedGuidApiResponse>("api/patient/insurance/profiles", request, cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess || result.Data is null)
            return Response<Guid>.Failure(result.ErrorMessage ?? "Create failed.", result.StatusCode);
        return Response<Guid>.Success(result.Data.Id);
    }

    public async Task<Response<bool>> UpdateProfileAsync(UpdatePatientInsuranceProfileRequest request, CancellationToken cancellationToken = default)
    {
        var result = await PutAsync<object>($"api/patient/insurance/profiles/{request.Id}", request, cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess)
            return Response<bool>.Failure(result.ErrorMessage ?? "Update failed.", result.StatusCode);
        return Response<bool>.Success(true);
    }
}
