using MediatR;

namespace RaphCare.Application.Features.Ops.Queries.GetPlatformOpsStats;

public sealed class GetPlatformOpsStatsQuery : IRequest<PlatformOpsStatsDto?>;
