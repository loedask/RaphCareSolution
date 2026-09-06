using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Features.Devices.Commands.AssignDeviceToPatient;
using RaphCare.Application.Features.Devices.Commands.CreateDevice;
using RaphCare.Application.Features.Devices.Commands.DeleteDevice;
using RaphCare.Application.Features.Devices.Commands.UnassignDeviceFromPatient;
using RaphCare.Application.Features.Devices.Commands.UpdateDevice;
using RaphCare.Application.Features.Devices.DTOs;
using RaphCare.Application.Features.Devices.Queries.GetDeviceById;
using RaphCare.Application.Features.Devices.Queries.GetDevices;

namespace RaphCare.API.Controllers;

/// <summary>Platform fleet device registry. Create and assign require platform admin.</summary>
[ApiController]
[Route("api/[controller]")]
public class DevicesController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id:guid}", Name = "GetDeviceById")]
    [Authorize(Policy = "RequireProvider")]
    [ProducesResponseType(typeof(DeviceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetDeviceByIdQuery { Id = id }, cancellationToken);
        return Ok(result);
    }

    [HttpGet(Name = "GetDevices")]
    [Authorize(Policy = "RequirePlatformAdmin")]
    [ProducesResponseType(typeof(PagedResult<DeviceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? clinicId = null,
        [FromQuery] bool? unassignedOnly = null,
        [FromQuery] string? serialContains = null,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new GetDevicesQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                ClinicId = clinicId,
                UnassignedOnly = unassignedOnly,
                SerialContains = serialContains
            },
            cancellationToken);
        return Ok(result);
    }

    [HttpPost(Name = "CreateDevice")]
    [Authorize(Policy = "RequirePlatformAdmin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateDeviceCommand command, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id }, new { id });
    }

    [HttpPost("{id:guid}/assign", Name = "AssignDeviceToPatient")]
    [Authorize(Policy = "RequirePlatformAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Assign(
        Guid id,
        [FromBody] AssignDeviceToPatientCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.DeviceId && command.DeviceId != Guid.Empty)
            return BadRequest();
        command.DeviceId = id;
        var deviceId = await mediator.Send(command, cancellationToken);
        return Ok(new { id = deviceId });
    }

    [HttpPost("{id:guid}/unassign", Name = "UnassignDeviceFromPatient")]
    [Authorize(Policy = "RequirePlatformAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unassign(Guid id, CancellationToken cancellationToken)
    {
        var deviceId = await mediator.Send(new UnassignDeviceFromPatientCommand { DeviceId = id }, cancellationToken);
        return Ok(new { id = deviceId });
    }

    [HttpDelete("{id:guid}", Name = "DeleteDevice")]
    [Authorize(Policy = "RequirePlatformAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteDeviceCommand { DeviceId = id }, cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:guid}", Name = "UpdateDevice")]
    [Authorize(Policy = "RequirePlatformAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDeviceCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest();
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
