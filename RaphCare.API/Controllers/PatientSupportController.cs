using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaphCare.Application.Features.PatientSupport.Commands.SubmitPatientSupportMessage;
using RaphCare.Application.Features.PatientSupport.DTOs;
using RaphCare.Application.Features.PatientSupport.Queries.GetPatientSupportContent;

namespace RaphCare.API.Controllers;

/// <summary>Patient help &amp; support (<c>api/patient/support</c>).</summary>
[Authorize(Policy = "RequirePatient")]
[ApiController]
[Route("api/patient/support")]
public sealed class PatientSupportController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpGet(Name = "GetPatientSupportContent")]
    [ProducesResponseType(typeof(PatientSupportContentDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetContent(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPatientSupportContentQuery(), cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("messages", Name = "SubmitPatientSupportMessage")]
    [ProducesResponseType(typeof(SubmitPatientSupportMessageResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> SubmitMessage(
        [FromBody] SubmitPatientSupportMessageCommand command,
        CancellationToken cancellationToken)
    {
        var ticketId = await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return Ok(new SubmitPatientSupportMessageResult { TicketId = ticketId });
    }
}

public sealed class SubmitPatientSupportMessageResult
{
    public Guid TicketId { get; set; }
}
