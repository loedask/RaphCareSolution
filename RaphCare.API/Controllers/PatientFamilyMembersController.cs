using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaphCare.API.App.Contracts;
using RaphCare.Application.Features.PatientFamilyMembers.Commands.AddMyPatientFamilyMember;
using RaphCare.Application.Features.PatientFamilyMembers.Commands.RemoveMyPatientFamilyMember;
using RaphCare.Application.Features.PatientFamilyMembers.Commands.UpdateMyPatientFamilyMember;
using RaphCare.Application.Features.PatientFamilyMembers.DTOs;
using RaphCare.Application.Features.PatientFamilyMembers.Queries.GetMyPatientFamilyMemberById;
using RaphCare.Application.Features.PatientFamilyMembers.Queries.GetMyPatientFamilyMembers;

namespace RaphCare.API.Controllers;

/// <summary>People linked to the patient account (concept <c>/family-members</c>).</summary>
[Authorize(Policy = "RequirePatient")]
[ApiController]
[Route("api/patient/family-members")]
public sealed class PatientFamilyMembersController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpGet(Name = "GetMyPatientFamilyMembers")]
    [ProducesResponseType(typeof(IReadOnlyList<PatientFamilyMemberDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyPatientFamilyMembersQuery(), cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpGet("{id:guid}", Name = "GetMyPatientFamilyMemberById")]
    [ProducesResponseType(typeof(PatientFamilyMemberDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyPatientFamilyMemberByIdQuery { Id = id }, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost(Name = "AddMyPatientFamilyMember")]
    [ProducesResponseType(typeof(CreatedGuidResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Add([FromBody] AddMyPatientFamilyMemberCommand command, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return CreatedAtRoute("GetMyPatientFamilyMemberById", new { id }, new CreatedGuidResponse { Id = id });
    }

    [HttpPut("{id:guid}", Name = "UpdateMyPatientFamilyMember")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMyPatientFamilyMemberCommand body, CancellationToken cancellationToken)
    {
        body.Id = id;
        await _mediator.Send(body, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    [HttpDelete("{id:guid}", Name = "RemoveMyPatientFamilyMember")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Remove(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RemoveMyPatientFamilyMemberCommand { Id = id }, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }
}
