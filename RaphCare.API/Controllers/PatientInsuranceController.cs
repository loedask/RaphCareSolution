using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaphCare.API.App.Contracts;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Features.PatientInsurance.Commands.CreatePatientInsuranceProfile;
using RaphCare.Application.Features.PatientInsurance.Commands.UpdateMyInsuranceProfile;
using RaphCare.Application.Features.PatientInsurance.DTOs;
using RaphCare.Application.Features.PatientInsurance.Queries.GetActiveInsurancePlans;
using RaphCare.Application.Features.PatientInsurance.Queries.GetMyInsuranceProfileById;
using RaphCare.Application.Features.PatientInsurance.Queries.GetMyInsuranceProfiles;

namespace RaphCare.API.Controllers;

/// <summary>Patient-scoped insurance profiles and plan catalog (JWT with <c>patientId</c> claim).</summary>
[Authorize(Policy = "RequirePatient")]
[ApiController]
[Route("api/patient/insurance")]
public class PatientInsuranceController(IMediator mediator) : ControllerBase
{
    [HttpGet("plans", Name = "GetActiveInsurancePlansForPatient")]
    [ProducesResponseType(typeof(IReadOnlyList<InsurancePlanOptionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPlans(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetActiveInsurancePlansQuery(), cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpGet("profiles", Name = "GetMyInsuranceProfiles")]
    [ProducesResponseType(typeof(PagedResult<PatientInsuranceProfileDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProfiles([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetMyInsuranceProfilesQuery { PageNumber = pageNumber, PageSize = pageSize }, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpGet("profiles/{id:guid}", Name = "GetMyInsuranceProfileById")]
    [ProducesResponseType(typeof(PatientInsuranceProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetMyInsuranceProfileByIdQuery { Id = id }, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("profiles", Name = "CreatePatientInsuranceProfile")]
    [ProducesResponseType(typeof(CreatedGuidResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreatePatientInsuranceProfileCommand command, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return CreatedAtRoute("GetMyInsuranceProfileById", new { id }, new CreatedGuidResponse { Id = id });
    }

    [HttpPut("profiles/{id:guid}", Name = "UpdateMyInsuranceProfile")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMyInsuranceProfileCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest();
        await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }
}
