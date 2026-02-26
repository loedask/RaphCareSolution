using MediatR;
using Microsoft.AspNetCore.Mvc;
using RaphCare.Application.Features.AI.Commands.GenerateSummary;

namespace RaphCare.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AIController : ControllerBase
{
    private readonly IMediator _mediator;

    public AIController(IMediator mediator) => _mediator = mediator;

    [HttpPost("summary")]
    public async Task<IActionResult> GenerateSummary([FromBody] GenerateSummaryCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}
