using MediatR;
using Microsoft.AspNetCore.Mvc;
using RaphCare.Application.Features.Telemedicine.Commands.CreateTeleSession;
using RaphCare.Application.Features.Telemedicine.Queries.GetTeleSessionById;

namespace RaphCare.API.Controllers;

/// <summary>Telemedicine sessions. Thin API; delegates to MediatR. Roles: Admin, Provider, Patient.</summary>
[ApiController]
[Route("api/[controller]")]
public class TelemedicineController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpGet("sessions/{id:guid}")]
    public async Task<IActionResult> GetSession(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetTeleSessionByIdQuery { Id = id }, ct);
        return Ok(result);
    }

    [HttpPost("sessions")]
    public async Task<IActionResult> CreateSession([FromBody] CreateTeleSessionCommand command, CancellationToken ct)
    {
        var id = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetSession), new { id }, new { id });
    }
}
