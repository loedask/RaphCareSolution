using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Features.Organization.Commands.EnsureClinicMembership;
using RaphCare.Application.Features.Organization.Commands.RegisterClinic;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Application.Features.Organization.Queries.GetAdminClinicById;
using RaphCare.Application.Features.Organization.Queries.GetAdminClinics;

namespace RaphCare.API.Controllers;

/// <summary>Practice portal: hospital onboarding and directory. Does not require X-Clinic-Id.</summary>
[Authorize(Policy = "RequireProvider")]
[ApiController]
[Route("api/admin/clinics")]
public class AdminClinicsController(IMediator mediator) : ControllerBase
{
    /// <summary>List hospitals the signed-in professional is linked to.</summary>
    [HttpGet(Name = "GetAdminClinics")]
    [ProducesResponseType(typeof(PagedResult<ClinicListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new GetAdminClinicsQuery { PageNumber = pageNumber, PageSize = pageSize },
            cancellationToken);
        return Ok(result);
    }

    /// <summary>Get one hospital (must be linked to the current user).</summary>
    [HttpGet("{id:guid}", Name = "GetAdminClinicById")]
    [ProducesResponseType(typeof(ClinicDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAdminClinicByIdQuery { ClinicId = id }, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Link the current user to a hospital (idempotent).</summary>
    [HttpPost("{id:guid}/membership", Name = "EnsureClinicMembership")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> EnsureMembership(Guid id, CancellationToken cancellationToken)
    {
        var linked = await mediator.Send(new EnsureClinicMembershipCommand { ClinicId = id }, cancellationToken);
        return linked ? NoContent() : Forbid();
    }

    /// <summary>Link the current user to an existing hospital by registration number (first claim only).</summary>
    [HttpPost("claim", Name = "ClaimClinicByRegistrationNumber")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ClaimByRegistrationNumber(
        [FromBody] ClaimClinicByRegistrationNumberRequest body,
        CancellationToken cancellationToken)
    {
        var clinicId = await mediator.Send(
            new ClaimClinicByRegistrationNumberCommand { RegistrationNumber = body.RegistrationNumber },
            cancellationToken);
        return clinicId is null ? NotFound() : Ok(new { clinicId });
    }

    /// <summary>Onboard a new hospital with optional primary facility.</summary>
    [HttpPost(Name = "RegisterClinic")]
    [ProducesResponseType(typeof(RegisterClinicResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterClinicCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetList), new { id = result.ClinicId }, result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("registration number", StringComparison.OrdinalIgnoreCase))
        {
            return Conflict(new { error = ex.Message });
        }
    }
}
