using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaphCare.Application.Features.PatientAiAssistant.Commands.SendMyPatientAssistantMessage;
using RaphCare.Application.Features.PatientAiAssistant.DTOs;

namespace RaphCare.API.Controllers;

/// <summary>Patient AI assistant (concept <c>/ai-assistant</c>). User message only—no automatic clinical context.</summary>
[Authorize(Policy = "RequirePatient")]
[ApiController]
[Route("api/patient/ai-assistant")]
public sealed class PatientAiAssistantController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpPost("chat", Name = "SendMyPatientAssistantMessage")]
    [ProducesResponseType(typeof(PatientAssistantReplyDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Chat([FromBody] SendMyPatientAssistantMessageCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }
}
