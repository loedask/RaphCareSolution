using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Features.Organization.Commands.RegisterClinic;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Application.Features.Organization.Queries.GetAdminClinics;

namespace RaphCare.API.Controllers;

/// <summary>Platform admin: hospital (clinic) onboarding and directory. Does not require X-Clinic-Id.</summary>
[Authorize(Policy = "RequirePlatformAdmin")]
[ApiController]
[Route("api/admin/clinics")]
public class AdminClinicsController(IMediator mediator) : ControllerBase
{
    /// <summary>List registered hospitals for the admin panel.</summary>
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
