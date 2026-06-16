using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaphCare.Application.Features.Auth.Commands.EmailAuth;

namespace RaphCare.API.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/auth/email")]
public class EmailAuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterWithEmailCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        if (!result.Success)
            return BadRequest(new { error = result.Error ?? "Registration failed." });
        return Ok(new { success = true, token = result.Token });
    }

    [HttpPost("signin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SignIn([FromBody] SignInWithEmailCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        if (!result.Success)
            return BadRequest(new { error = result.Error ?? "Sign-in failed." });
        return Ok(new { success = true, token = result.Token });
    }
}
