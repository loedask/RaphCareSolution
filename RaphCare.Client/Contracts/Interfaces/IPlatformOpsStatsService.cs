using RaphCare.Client.Contracts;
using RaphCare.Client.Models.Ops;

namespace RaphCare.Client.Contracts.Interfaces;

public interface IPlatformOpsStatsService
{
    Task<Response<PlatformOpsStats>> GetStatsAsync(CancellationToken cancellationToken = default);
}
