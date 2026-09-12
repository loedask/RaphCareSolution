using RaphCare.Client.Contracts;
using RaphCare.Client.Models;

namespace RaphCare.Client.Contracts.Interfaces;

public interface IConsultDisplayService
{
    Task<Response<ConsultDisplayBoard>> GetBoardAsync(string token, CancellationToken cancellationToken = default);
}
