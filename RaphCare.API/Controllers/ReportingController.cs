using MediatR;
using Microsoft.AspNetCore.Mvc;
using RaphCare.Application.Features.Reporting.Queries.GetDashboardSnapshot;

namespace RaphCare.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportingController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>Get dashboard snapshot for a clinic. Roles: Admin, Provider.</summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardSnapshot([FromQuery] Guid clinicId, [FromQuery] DateTime? snapshotDate, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDashboardSnapshotQuery { ClinicId = clinicId, SnapshotDate = snapshotDate ?? DateTime.UtcNow.Date }, cancellationToken);
        return Ok(result);
    }
}
