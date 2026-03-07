using MediatR;
using Microsoft.AspNetCore.Mvc;
using RaphCare.Application.Features.Patients.Commands.CreatePatient;
using RaphCare.Application.Features.Patients.Commands.UpdatePatient;
using RaphCare.Application.Features.Patients.DTOs;
using RaphCare.Application.Features.Patients.Queries.GetPatientById;
using RaphCare.Application.Features.Patients.Queries.GetPatients;
using RaphCare.Application.Common.DTOs;
using RaphCare.API.App.Contracts;

namespace RaphCare.API.Controllers;

/// <summary>
/// Patient registration and retrieval. Roles: Admin, Provider.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PatientsController(IMediator mediator) : ControllerBase
{
    /// <summary>Get a patient by id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PatientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPatientByIdQuery { Id = id }, cancellationToken);
        return Ok(result);
    }

    /// <summary>Get paginated list of patients.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<PatientDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetPatientsQuery { PageNumber = pageNumber, PageSize = pageSize }, cancellationToken);
        return Ok(result);
    }

    /// <summary>Create a new patient.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreatePatientResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreatePatientCommand command, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id }, new { id });
    }

    /// <summary>Update an existing patient.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePatientCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest();
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
