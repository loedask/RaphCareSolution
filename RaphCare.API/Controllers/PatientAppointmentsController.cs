using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaphCare.API.App.Contracts;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Features.Appointments.Commands.CreatePatientAppointment;
using RaphCare.Application.Features.Appointments.DTOs;
using RaphCare.Application.Features.Appointments.Queries.GetBookableProviders;
using RaphCare.Application.Features.Appointments.Queries.GetMyAppointmentById;
using RaphCare.Application.Features.Appointments.Queries.GetMyAppointments;

namespace RaphCare.API.Controllers;

/// <summary>Patient-scoped appointments (JWT with <c>patientId</c> claim). Mobile / patient portal.</summary>
[Authorize(Policy = "RequirePatient")]
[ApiController]
[Route("api/patient/appointments")]
public class PatientAppointmentsController(IMediator mediator) : ControllerBase
{
    [HttpGet(Name = "GetMyAppointments")]
    [ProducesResponseType(typeof(PagedResult<AppointmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetMyAppointmentsQuery { PageNumber = pageNumber, PageSize = pageSize }, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpGet("providers", Name = "GetBookableProviders")]
    [ProducesResponseType(typeof(IReadOnlyList<BookableProviderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProviders(CancellationToken cancellationToken)
    {
        var providers = await mediator.Send(new GetBookableProvidersQuery(), cancellationToken).ConfigureAwait(false);
        return Ok(providers);
    }

    [HttpGet("{id:guid}", Name = "GetMyAppointmentById")]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetMyAppointmentByIdQuery { Id = id }, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost(Name = "CreatePatientAppointment")]
    [ProducesResponseType(typeof(CreatedGuidResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreatePatientAppointmentCommand command, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return CreatedAtRoute("GetMyAppointmentById", new { id }, new CreatedGuidResponse { Id = id });
    }
}
