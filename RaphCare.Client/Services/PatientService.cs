using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models;
using RaphCare.Client.Models.Api;
using RaphCare.Client.Models.Appointments;
using RaphCare.Client.Models.Patients;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client.Services;

public sealed class PatientService(HttpClient httpClient) : BaseHttpService(httpClient), IPatientService
{
    public Task<Response<PatientViewModel?>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        GetAsync<PatientViewModel?>($"api/Patients/{id}", cancellationToken);

    public async Task<Response<PagedResultViewModel<PatientViewModel>>> GetListAsync(
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await GetAsync<PagedApiResult<PatientViewModel>>(
                $"api/Patients?pageNumber={pageNumber}&pageSize={pageSize}",
                cancellationToken)
            .ConfigureAwait(false);

        if (!result.IsSuccess || result.Data is null)
            return Response<PagedResultViewModel<PatientViewModel>>.Failure(
                result.ErrorMessage ?? "Could not load patients.",
                result.StatusCode);

        return Response<PagedResultViewModel<PatientViewModel>>.Success(new PagedResultViewModel<PatientViewModel>
        {
            Items = result.Data.Items ?? Array.Empty<PatientViewModel>(),
            TotalCount = result.Data.TotalCount,
            PageNumber = result.Data.PageNumber,
            PageSize = result.Data.PageSize
        });
    }

    public async Task<Response<Guid>> CreateAsync(CreatePatientRequest request, CancellationToken cancellationToken = default)
    {
        var result = await PostAsync<CreatedGuidApiResponse>("api/Patients", request, cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess || result.Data is null)
            return Response<Guid>.Failure(result.ErrorMessage ?? "Create failed.", result.StatusCode);
        return Response<Guid>.Success(result.Data.Id);
    }

    public async Task<Response<bool>> UpdateAsync(Guid id, UpdatePatientRequest request, CancellationToken cancellationToken = default)
    {
        request.Id = id;
        var result = await PutNoContentAsync($"api/Patients/{id}", request, cancellationToken).ConfigureAwait(false);
        return result.IsSuccess
            ? Response<bool>.Success(true)
            : Response<bool>.Failure(result.ErrorMessage ?? "Update failed.", result.StatusCode);
    }
}
