using RaphCare.Client.Contracts;
using RaphCare.Client.Models;

namespace RaphCare.Client.Contracts.Interfaces;

public interface ICasualtyDisplayService
{
    Task<Response<CasualtyDisplayBoard>> GetBoardAsync(string token, CancellationToken cancellationToken = default);
}
