using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaphCare.Application.Features.Interoperability.Queries.GetFhirPatientById;
using RaphCare.Application.Features.Interoperability.Queries.GetFhirEncounterById;
using RaphCare.Application.Features.Interoperability.Queries.GetFhirOrganizationById;

namespace RaphCare.API.Controllers;

/// <summary>
/// Phase 1 FHIR-shaped exports.
/// Security: Provider/Admin only. Patient-facing access is intentionally not exposed yet.
/// </summary>
[Authorize(Policy = "RequireProvider")]
[ApiController]
[Route("api/fhir")]
public class FhirExportController(IMediator mediator) : ControllerBase
{
    // TODO: future support for content negotiation (application/fhir+json).

    [HttpGet("patients/{id:guid}", Name = "GetFhirPatientById")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPatient([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetFhirPatientByIdQuery { Id = id }, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpGet("encounters/{id:guid}", Name = "GetFhirEncounterById")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEncounter([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetFhirEncounterByIdQuery { Id = id }, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpGet("organizations/{id:guid}", Name = "GetFhirOrganizationById")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrganization([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetFhirOrganizationByIdQuery { Id = id }, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }
}

