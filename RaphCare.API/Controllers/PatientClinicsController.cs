using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaphCare.Application.Features.PatientClinics.DTOs;
using RaphCare.Application.Features.PatientClinics.Queries.GetMyLinkedClinics;

namespace RaphCare.API.Controllers;

/// <summary>
/// Patient hospitals they are linked to (membership / care history), separate from Active clinic on the phone.
/// Does not require <c>X-Clinic-Id</c> (tenant-exempt) so the list loads before a hospital is picked.
/// </summary>
[Authorize(Policy = "RequirePatient")]
[ApiController]
[Route("api/patient/clinics")]
public sealed class PatientClinicsController(IMediator mediator) : ControllerBase
{
    [HttpGet(Name = "GetMyLinkedClinics")]
    [ProducesResponseType(typeof(IReadOnlyList<PatientLinkedClinicDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLinkedClinics(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetMyLinkedClinicsQuery(), cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }
}
