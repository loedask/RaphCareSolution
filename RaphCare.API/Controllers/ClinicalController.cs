using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaphCare.Application.Features.Clinical.Commands.CreateVisit;
using RaphCare.Application.Features.Clinical.Commands.UpdateVisit;
using RaphCare.Application.Features.Clinical.Queries.GetVisitById;
using RaphCare.Application.Features.Clinical.Queries.GetVisits;

namespace RaphCare.API.Controllers;

/// <summary>Clinical visits and documentation. Thin API; delegates to MediatR. Roles: Admin, Provider.</summary>
[Authorize(Policy = "RequireProvider")]
[ApiController]
[Route("api/[controller]")]
public class ClinicalController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpGet("visits/{id:guid}")]
    public async Task<IActionResult> GetVisit(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetVisitByIdQuery { Id = id }, cancellationToken);
        return Ok(result);
    }

    [HttpGet("visits")]
    public async Task<IActionResult> GetVisits(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetVisitsQuery { PageNumber = pageNumber, PageSize = pageSize }, cancellationToken);
        return Ok(result);
    }

    [HttpPost("visits")]
    public async Task<IActionResult> CreateVisit([FromBody] CreateVisitCommand command, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetVisit), new { id }, new { id });
    }

    [HttpPut("visits/{id:guid}")]
    public async Task<IActionResult> UpdateVisit(Guid id, [FromBody] UpdateVisitCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest();
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
