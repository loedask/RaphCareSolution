using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaphCare.Application.Features.Auth.Commands.SendOtp;
using RaphCare.Application.Features.Auth.Commands.VerifyOtp;

namespace RaphCare.API.Controllers;

/// <summary>
/// Phone-based OTP authentication endpoints for patient onboarding and low-friction access.
/// </summary>
[AllowAnonymous]
[ApiController]
[Route("api/auth/otp")]
public class AuthController(IMediator mediator) : ControllerBase
{
    /// <summary>Send a one-time password (OTP) to the specified phone number.</summary>
    [HttpPost("send")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Send([FromBody] SendOtpCommand command, CancellationToken cancellationToken)
    {
        try
        {
            await mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return StatusCode(StatusCodes.Status429TooManyRequests);
        }
    }

    /// <summary>Verify an OTP for a phone number.</summary>
    [HttpPost("verify")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Verify([FromBody] VerifyOtpCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(new { error = "Invalid or expired code." });
        }

        return Ok(new { success = true, token = result.Token });
    }
}

