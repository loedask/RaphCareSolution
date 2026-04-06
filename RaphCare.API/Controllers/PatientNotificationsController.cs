using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaphCare.Application.Features.PatientNotifications.Commands.MarkAllMyPatientNotificationsRead;
using RaphCare.Application.Features.PatientNotifications.Commands.MarkMyPatientNotificationRead;
using RaphCare.Application.Features.PatientNotifications.Commands.RegisterMyPatientPushDevice;
using RaphCare.Application.Features.PatientNotifications.DTOs;
using RaphCare.Application.Features.PatientNotifications.Queries.GetMyPatientNotifications;

namespace RaphCare.API.Controllers;

/// <summary>In-app notification center and push device registration (<c>api/patient/notifications</c>).</summary>
[Authorize(Policy = "RequirePatient")]
[ApiController]
[Route("api/patient/notifications")]
public sealed class PatientNotificationsController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpGet(Name = "GetMyPatientNotifications")]
    [ProducesResponseType(typeof(IReadOnlyList<PatientNotificationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyPatientNotificationsQuery(), cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPut("{id:guid}/read", Name = "MarkMyPatientNotificationRead")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new MarkMyPatientNotificationReadCommand { Id = id }, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    [HttpPut("read-all", Name = "MarkAllMyPatientNotificationsRead")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
    {
        await _mediator.Send(new MarkAllMyPatientNotificationsReadCommand(), cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    [HttpPut("push-device", Name = "RegisterMyPatientPushDevice")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RegisterPushDevice([FromBody] RegisterMyPatientPushDeviceCommand command, CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }
}
