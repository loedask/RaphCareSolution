using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaphCare.API.App.Contracts;
using RaphCare.Application.Features.PatientEmergencyContacts.Commands.AddMyPatientEmergencyContact;
using RaphCare.Application.Features.PatientEmergencyContacts.Commands.RemoveMyPatientEmergencyContact;
using RaphCare.Application.Features.PatientEmergencyContacts.Commands.UpdateMyPatientEmergencyContact;
using RaphCare.Application.Features.PatientEmergencyContacts.DTOs;
using RaphCare.Application.Features.PatientEmergencyContacts.Queries.GetMyPatientEmergencyContactById;
using RaphCare.Application.Features.PatientEmergencyContacts.Queries.GetMyPatientEmergencyContacts;

namespace RaphCare.API.Controllers;

/// <summary>ICE contacts for the signed-in patient (concept <c>/emergency-contacts</c>).</summary>
[Authorize(Policy = "RequirePatient")]
[ApiController]
[Route("api/patient/emergency-contacts")]
public sealed class PatientEmergencyContactsController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpGet(Name = "GetMyPatientEmergencyContacts")]
    [ProducesResponseType(typeof(IReadOnlyList<PatientEmergencyContactDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyPatientEmergencyContactsQuery(), cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpGet("{id:guid}", Name = "GetMyPatientEmergencyContactById")]
    [ProducesResponseType(typeof(PatientEmergencyContactDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyPatientEmergencyContactByIdQuery { Id = id }, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost(Name = "AddMyPatientEmergencyContact")]
    [ProducesResponseType(typeof(CreatedGuidResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Add([FromBody] AddMyPatientEmergencyContactCommand command, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return CreatedAtRoute("GetMyPatientEmergencyContactById", new { id }, new CreatedGuidResponse { Id = id });
    }

    [HttpPut("{id:guid}", Name = "UpdateMyPatientEmergencyContact")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMyPatientEmergencyContactCommand body, CancellationToken cancellationToken)
    {
        body.Id = id;
        await _mediator.Send(body, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    [HttpDelete("{id:guid}", Name = "RemoveMyPatientEmergencyContact")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Remove(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RemoveMyPatientEmergencyContactCommand { Id = id }, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }
}
