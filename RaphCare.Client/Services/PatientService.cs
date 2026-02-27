using AutoMapper;
using RaphCare.Client.Contracts;
using RaphCare.Client.Generated;
using RaphCare.Client.Models;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client.Services;

public class PatientService : BaseHttpService, IPatientService
{
    private readonly IMapper _mapper;

    public PatientService(IClient client, HttpClient httpClient, IMapper mapper)
        : base(client, httpClient)
    {
        _mapper = mapper;
    }

    public async Task<Response<PatientViewModel?>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var dto = await Client.GetPatientAsync(id, cancellationToken).ConfigureAwait(false);
            if (dto is null)
                return Response<PatientViewModel?>.Failure("Patient not found.", 404);
            var viewModel = _mapper.Map<PatientViewModel>(dto);
            return Response<PatientViewModel?>.Success(viewModel);
        }
        catch (ApiException ex)
        {
            return Response<PatientViewModel?>.Failure(ex.Message, ex.StatusCode);
        }
    }

    public async Task<Response<PagedResultViewModel<PatientViewModel>>> GetListAsync(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        try
        {
            var dto = await Client.GetPatientsAsync(pageNumber, pageSize, cancellationToken).ConfigureAwait(false);
            if (dto is null)
                return Response<PagedResultViewModel<PatientViewModel>>.Failure("Failed to load patients.");
            var viewModel = _mapper.Map<PagedResultViewModel<PatientViewModel>>(dto);
            return Response<PagedResultViewModel<PatientViewModel>>.Success(viewModel);
        }
        catch (ApiException ex)
        {
            return Response<PagedResultViewModel<PatientViewModel>>.Failure(ex.Message, ex.StatusCode);
        }
    }

    public async Task<Response<Guid>> CreateAsync(CreatePatientRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var command = _mapper.Map<CreatePatientCommand>(request);
            var result = await Client.CreatePatientAsync(command, cancellationToken).ConfigureAwait(false);
            if (result is null)
                return Response<Guid>.Failure("Create returned no result.");
            return Response<Guid>.Success(result.Id);
        }
        catch (ApiException ex)
        {
            return Response<Guid>.Failure(ex.Message, ex.StatusCode);
        }
    }

    public async Task<Response<bool>> UpdateAsync(Guid id, UpdatePatientRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            request.Id = id;
            var command = _mapper.Map<UpdatePatientCommand>(request);
            await Client.UpdatePatientAsync(id, command, cancellationToken).ConfigureAwait(false);
            return Response<bool>.Success(true);
        }
        catch (ApiException ex)
        {
            return Response<bool>.Failure(ex.Message, ex.StatusCode);
        }
    }
}
