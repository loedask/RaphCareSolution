using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaphCare.Application.Features.PatientProfile.Commands.UpdateMyPatientProfile;
using RaphCare.Application.Features.PatientProfile.DTOs;
using RaphCare.Application.Features.PatientProfile.Queries.GetMyPatientProfile;

namespace RaphCare.API.Controllers;

/// <summary>Current patient demographics (<c>api/patient/profile</c>).</summary>
[Authorize(Policy = "RequirePatient")]
[ApiController]
[Route("api/patient/profile")]
public sealed class PatientProfileController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpGet(Name = "GetMyPatientProfile")]
    [ProducesResponseType(typeof(MyPatientProfileDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyPatientProfileQuery(), cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPut(Name = "UpdateMyPatientProfile")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update([FromBody] UpdateMyPatientProfileCommand command, CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }
}
