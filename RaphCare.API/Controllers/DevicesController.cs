using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaphCare.Application.Features.Devices.Commands.CreateDevice;
using RaphCare.Application.Features.Devices.Commands.UpdateDevice;
using RaphCare.Application.Features.Devices.Queries.GetDeviceById;
using RaphCare.Application.Features.Devices.Queries.GetDevices;

namespace RaphCare.API.Controllers;

/// <summary>Device registry and management. Thin API; delegates to MediatR. Roles: Admin, Provider.</summary>
[Authorize(Policy = "RequireProvider")]
[ApiController]
[Route("api/[controller]")]
public class DevicesController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetDeviceByIdQuery { Id = id }, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetDevicesQuery { PageNumber = pageNumber, PageSize = pageSize }, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDeviceCommand command, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDeviceCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest();
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
