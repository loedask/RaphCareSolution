using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Features.PatientTelehealth.Commands.SendTelehealthSessionSms;
using RaphCare.Application.Features.PatientTelehealth.DTOs;
using RaphCare.Application.Features.PatientTelehealth.Queries.GetMyTeleSessions;
using RaphCare.Application.Features.PatientTelehealth.Queries.GetTelehealthJoinInfo;

namespace RaphCare.API.Controllers;

/// <summary>Patient telehealth: list sessions, Agora join credentials, Twilio SMS reminder.</summary>
[Authorize(Policy = "RequirePatient")]
[ApiController]
[Route("api/patient/telehealth")]
public class PatientTelehealthController(IMediator mediator) : ControllerBase
{
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
