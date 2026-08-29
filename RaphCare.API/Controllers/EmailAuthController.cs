using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaphCare.Application.Features.Auth.Commands.EmailAuth;
using RaphCare.Application.Features.Auth.Queries.GetRegistrationClinics;

namespace RaphCare.API.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/auth/email")]
public class EmailAuthController(IMediator mediator) : ControllerBase
{
    [HttpGet("clinics")]
    [ProducesResponseType(typeof(IReadOnlyList<RegistrationClinicDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRegistrationClinics(
        [FromQuery] string? q,
        CancellationToken cancellationToken)
    {
        var clinics = await mediator
            .Send(new GetRegistrationClinicsQuery { Search = q }, cancellationToken)
            .ConfigureAwait(false);
        return Ok(clinics);
    }

    [HttpGet("clinics/by-reference")]
    [ProducesResponseType(typeof(RegistrationClinicDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResolveClinicByReference(
        [FromQuery] string code,
        CancellationToken cancellationToken)
    {
        var clinic = await mediator
            .Send(new ResolveRegistrationClinicByReferenceQuery { ReferenceCode = code }, cancellationToken)
            .ConfigureAwait(false);
        if (clinic is null)
            return NotFound(new { error = "No clinic found for that reference code." });
        return Ok(clinic);
    }

    [HttpPost("send-verification")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> SendVerification(
        [FromBody] SendEmailVerificationCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            await mediator.Send(command, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("limit", StringComparison.OrdinalIgnoreCase))
        {
            return StatusCode(StatusCodes.Status429TooManyRequests, new { error = ex.Message });
        }
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterWithEmailCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return AuthResult(result);
    }

    [HttpPost("signin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SignIn([FromBody] SignInWithEmailCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return AuthResult(result);
    }

    [HttpPost("register-professional")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterProfessional(
        [FromBody] RegisterProfessionalWithEmailCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return AuthResult(result);
    }

    [HttpPost("signin-professional")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SignInProfessional(
        [FromBody] SignInProfessionalWithEmailCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return AuthResult(result);
    }

    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] RequestPasswordResetCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            await mediator.Send(command, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("limit", StringComparison.OrdinalIgnoreCase))
        {
            return StatusCode(StatusCodes.Status429TooManyRequests, new { error = ex.Message });
        }
    }

    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ConfirmPasswordResetCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return AuthResult(result);
    }

    private IActionResult AuthResult(EmailAuthResult result)
    {
        if (!result.Success)
            return BadRequest(new { error = result.Error ?? "Request failed." });

        return Ok(new
        {
            success = true,
            token = result.Token,
            requiresVerification = result.RequiresVerification
        });
    }
}
