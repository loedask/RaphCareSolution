using AutoMapper;
using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Telehealth;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client.Services;

/// <summary>Patient telehealth API via generated <see cref="IClient"/>.</summary>
public class PatientTelehealthService(IClient client, IMapper mapper) : IPatientTelehealthService
{
    private readonly IClient _client = client ?? throw new ArgumentNullException(nameof(client));
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

    public async Task<Response<PagedPatientTeleSessionsViewModel>> GetMySessionsAsync(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        try
        {
            var dto = await _client.GetMyTeleSessionsAsync(pageNumber, pageSize, cancellationToken).ConfigureAwait(false);
            var vm = _mapper.Map<PagedPatientTeleSessionsViewModel>(dto);
            return Response<PagedPatientTeleSessionsViewModel>.Success(vm);
        }
        catch (RaphCare.Client.Services.Base.ApiException ex)
        {
            return Response<PagedPatientTeleSessionsViewModel>.Failure(ex.Message, ex.StatusCode);
        }
    }

    public async Task<Response<TelehealthJoinInfoViewModel?>> GetJoinInfoAsync(Guid teleSessionId, int? uid = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var dto = await _client.GetTelehealthJoinInfoAsync(teleSessionId, uid, cancellationToken).ConfigureAwait(false);
            var vm = _mapper.Map<TelehealthJoinInfoViewModel>(dto);
            return Response<TelehealthJoinInfoViewModel?>.Success(vm);
        }
        catch (RaphCare.Client.Services.Base.ApiException ex)
        {
            return Response<TelehealthJoinInfoViewModel?>.Failure(ex.Message, ex.StatusCode);
        }
    }

    public async Task<Response<bool>> SendSessionSmsAsync(Guid teleSessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _client.SendTelehealthSessionSmsAsync(teleSessionId, cancellationToken).ConfigureAwait(false);
            return Response<bool>.Success(true);
        }
        catch (RaphCare.Client.Services.Base.ApiException ex)
        {
            return Response<bool>.Failure(ex.Message, ex.StatusCode);
        }
    }
}
