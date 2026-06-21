using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaphCare.Application.Features.PatientAccount.Commands.ChangeMyEmailPassword;

namespace RaphCare.API.Controllers;

/// <summary>Patient account settings (email password change for local credentials).</summary>
[Authorize(Policy = "RequirePatient")]
[ApiController]
[Route("api/patient/account")]
public sealed class PatientAccountController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpPost("change-password", Name = "ChangeMyEmailPassword")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangeMyEmailPasswordCommand command,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }
}
