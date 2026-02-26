using MediatR;
using Microsoft.AspNetCore.Mvc;
using RaphCare.Application.Features.Insurance.Commands.CreateInsuranceProfile;
using RaphCare.Application.Features.Insurance.Commands.UpdateInsuranceProfile;
using RaphCare.Application.Features.Insurance.Queries.GetInsuranceProfileById;
using RaphCare.Application.Features.Insurance.Queries.GetInsuranceProfiles;

namespace RaphCare.API.Controllers;

/// <summary>Insurance profiles and plans. Thin API; delegates to MediatR. Roles: Admin, Provider, Patient.</summary>
[ApiController]
[Route("api/[controller]")]
public class InsuranceController(IMediator mediator) : ControllerBase
{
    [HttpGet("profiles/{id:guid}")]
    public async Task<IActionResult> GetProfile(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetInsuranceProfileByIdQuery { Id = id }, cancellationToken);
        return Ok(result);
    }

    [HttpGet("profiles")]
    public async Task<IActionResult> GetProfiles([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetInsuranceProfilesQuery { PageNumber = pageNumber, PageSize = pageSize }, cancellationToken);
        return Ok(result);
    }

    [HttpPost("profiles")]
    public async Task<IActionResult> CreateProfile([FromBody] CreateInsuranceProfileCommand command, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetProfile), new { id }, new { id });
    }

    [HttpPut("profiles/{id:guid}")]
    public async Task<IActionResult> UpdateProfile(Guid id, [FromBody] UpdateInsuranceProfileCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest();
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
