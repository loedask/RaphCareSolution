using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaphCare.API.App.Contracts;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Features.PatientTelehealth.Commands.RequestOnDemandTelehealthSession;
using RaphCare.Application.Features.PatientTelehealth.Commands.SendTelehealthChatMessage;
using RaphCare.Application.Features.PatientTelehealth.Commands.SendTelehealthSessionSms;
using RaphCare.Application.Features.PatientTelehealth.DTOs;
using RaphCare.Application.Features.PatientTelehealth.Queries.GetMyTeleSessions;
using RaphCare.Application.Features.PatientTelehealth.Queries.GetTelehealthJoinInfo;
using RaphCare.Application.Features.PatientTelehealth.Queries.GetTelehealthSessionChat;

namespace RaphCare.API.Controllers;

/// <summary>Patient telehealth: list sessions, Agora join credentials, Twilio SMS reminder.</summary>
[Authorize(Policy = "RequirePatient")]
[ApiController]
[Route("api/patient/telehealth")]
public class PatientTelehealthController(IMediator mediator) : ControllerBase
{
    [HttpPost("sessions/request", Name = "RequestOnDemandTelehealthSession")]
    [ProducesResponseType(typeof(CreatedGuidResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RequestSession(
        [FromBody] RequestOnDemandTelehealthSessionCommand command,
        CancellationToken cancellationToken)
    {
        var id = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return CreatedAtRoute("GetTelehealthJoinInfo", new { teleSessionId = id }, new CreatedGuidResponse { Id = id });
    }

    [HttpGet("sessions/{teleSessionId:guid}/chat", Name = "GetTelehealthSessionChat")]
    [ProducesResponseType(typeof(IReadOnlyList<TelehealthChatMessageDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChat(Guid teleSessionId, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new GetTelehealthSessionChatQuery { TeleSessionId = teleSessionId, PageSize = pageSize },
            cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("sessions/{teleSessionId:guid}/chat", Name = "SendTelehealthChatMessage")]
    [ProducesResponseType(typeof(CreatedGuidResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> SendChat(
        Guid teleSessionId,
        [FromBody] SendTelehealthChatMessageCommand command,
        CancellationToken cancellationToken)
    {
        command.TeleSessionId = teleSessionId;
        var id = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return CreatedAtRoute("GetTelehealthSessionChat", new { teleSessionId }, new CreatedGuidResponse { Id = id });
    }

    [HttpGet("sessions", Name = "GetMyTeleSessions")]
    [ProducesResponseType(typeof(PagedResult<PatientTeleSessionListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSessions([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetMyTeleSessionsQuery { PageNumber = pageNumber, PageSize = pageSize }, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpGet("sessions/{teleSessionId:guid}/join-info", Name = "GetTelehealthJoinInfo")]
    [ProducesResponseType(typeof(TelehealthJoinInfoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetJoinInfo(Guid teleSessionId, [FromQuery] uint? uid, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetTelehealthJoinInfoQuery { TeleSessionId = teleSessionId, Uid = uid }, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("sessions/{teleSessionId:guid}/notify-sms", Name = "SendTelehealthSessionSms")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> NotifySms(Guid teleSessionId, CancellationToken cancellationToken)
    {
        await mediator.Send(new SendTelehealthSessionSmsCommand { TeleSessionId = teleSessionId }, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }
}
