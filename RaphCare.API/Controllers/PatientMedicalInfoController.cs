using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaphCare.Application.Features.PatientMedicalInfo.Commands.UpdateMyPatientMedicalInfo;
using RaphCare.Application.Features.PatientMedicalInfo.DTOs;
using RaphCare.Application.Features.PatientMedicalInfo.Queries.GetMyPatientMedicalInfo;

namespace RaphCare.API.Controllers;

/// <summary>Patient-reported medical summary (concept <c>/medical-information</c>).</summary>
[Authorize(Policy = "RequirePatient")]
[ApiController]
[Route("api/patient/medical-info")]
public sealed class PatientMedicalInfoController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpGet(Name = "GetMyPatientMedicalInfo")]
    [ProducesResponseType(typeof(MyPatientMedicalInfoDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyPatientMedicalInfoQuery(), cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPut(Name = "UpdateMyPatientMedicalInfo")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update([FromBody] UpdateMyPatientMedicalInfoCommand command, CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }
}
