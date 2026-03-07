using AutoMapper;
using RaphCare.Client.Contracts;
using RaphCare.Client.Models;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client.Services;

public class PatientService(IClient client, HttpClient httpClient, IMapper mapper) : BaseHttpService(client, httpClient), IPatientService
{
    private readonly IMapper _mapper = mapper;

    public async Task<Response<PatientViewModel?>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var dto = await Client.PatientsGETAsync(id, cancellationToken).ConfigureAwait(false);
            var viewModel = _mapper.Map<PatientViewModel>(dto);
            return Response<PatientViewModel?>.Success(viewModel);
        }
        catch (RaphCare.Client.Services.Base.ApiException ex)
        {
            return Response<PatientViewModel?>.Failure(ex.Message, ex.StatusCode);
        }
    }

    public async Task<Response<PagedResultViewModel<PatientViewModel>>> GetListAsync(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        try
        {
            var dto = await Client.PatientsGET2Async(pageNumber, pageSize, cancellationToken).ConfigureAwait(false);
            var viewModel = _mapper.Map<PagedResultViewModel<PatientViewModel>>(dto);
            return Response<PagedResultViewModel<PatientViewModel>>.Success(viewModel);
        }
        catch (RaphCare.Client.Services.Base.ApiException ex)
        {
            return Response<PagedResultViewModel<PatientViewModel>>.Failure(ex.Message, ex.StatusCode);
        }
    }

    public async Task<Response<Guid>> CreateAsync(CreatePatientRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var command = _mapper.Map<CreatePatientCommand>(request);
            var result = await Client.PatientsPOSTAsync(command, cancellationToken).ConfigureAwait(false);
            return Response<Guid>.Success(result.Id);
        }
        catch (RaphCare.Client.Services.Base.ApiException ex)
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
            await Client.PatientsPUTAsync(id, command, cancellationToken).ConfigureAwait(false);
            return Response<bool>.Success(true);
        }
        catch (RaphCare.Client.Services.Base.ApiException ex)
        {
            return Response<bool>.Failure(ex.Message, ex.StatusCode);
        }
    }
}
