using MediatR;
using Microsoft.AspNetCore.Mvc;
using RaphCare.Application.Features.AI.Commands.GenerateSummary;

namespace RaphCare.API.Controllers;

/// <summary>AI-powered summaries and insights. Thin API; delegates to MediatR. Roles: Admin, Provider.</summary>
[ApiController]
[Route("api/[controller]")]
public class AIController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpPost("summary")]
    public async Task<IActionResult> GenerateSummary([FromBody] GenerateSummaryCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}
