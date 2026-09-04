using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaphCare.Application.Features.Ops.Queries.GetPlatformOpsStats;

namespace RaphCare.API.Controllers;

/// <summary>Platform Ops dashboard endpoints (RequirePlatformAdmin).</summary>
[ApiController]
[Route("api/ops")]
[Authorize(Policy = "RequirePlatformAdmin")]
public sealed class OpsController(IMediator mediator) : ControllerBase
{
    /// <summary>Platform-wide counts for the Ops home screen.</summary>
    [HttpGet("stats", Name = "GetPlatformOpsStats")]
    [ProducesResponseType(typeof(PlatformOpsStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetStats(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPlatformOpsStatsQuery(), cancellationToken)
            .ConfigureAwait(false);
        if (result is null)
            return Forbid();
        return Ok(result);
    }
}
