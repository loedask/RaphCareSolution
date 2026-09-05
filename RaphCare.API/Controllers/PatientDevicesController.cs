using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaphCare.Application.Features.PatientDevices.Commands.BindMyDeviceBluetoothMac;
using RaphCare.Application.Features.PatientDevices.Commands.RegisterMyDevice;
using RaphCare.Application.Features.PatientDevices.Commands.SyncMyDeviceReadings;
using RaphCare.Application.Features.PatientDevices.DTOs;
using RaphCare.Application.Features.PatientDevices.Queries.GetMyDevices;
using RaphCare.Application.Features.PatientDevices.Queries.GetMyLatestReadings;

namespace RaphCare.API.Controllers;

/// <summary>Patient wearable registry and vitals sync (JWT with patient profile). BLE still pairs on-device; this persists readings server-side.</summary>
[Authorize(Policy = "RequirePatient")]
[ApiController]
[Route("api/patient/devices")]
public class PatientDevicesController(IMediator mediator) : ControllerBase
{
    [HttpGet(Name = "GetMyDevices")]
    [ProducesResponseType(typeof(IReadOnlyList<PatientDeviceListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyDevices(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetMyDevicesQuery(), cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpGet("readings/latest", Name = "GetMyLatestReadings")]
    [ProducesResponseType(typeof(GetMyLatestReadingsResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyLatestReadings(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetMyLatestReadingsQuery(), cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("register", Name = "RegisterMyDevice")]
    [ProducesResponseType(typeof(RegisterMyDeviceResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Register([FromBody] RegisterMyDeviceCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("{deviceId:guid}/bluetooth-mac", Name = "BindMyDeviceBluetoothMac")]
    [ProducesResponseType(typeof(BindMyDeviceBluetoothMacResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BindBluetoothMac(
        Guid deviceId,
        [FromBody] BindMyDeviceBluetoothMacCommand command,
        CancellationToken cancellationToken)
    {
        command.DeviceId = deviceId;
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("{deviceId:guid}/readings", Name = "SyncMyDeviceReadings")]
    [ProducesResponseType(typeof(SyncMyDeviceReadingsResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SyncReadings(Guid deviceId, [FromBody] SyncMyDeviceReadingsCommand command, CancellationToken cancellationToken)
    {
        command.DeviceId = deviceId;
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }
}
