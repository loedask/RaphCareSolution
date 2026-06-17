using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Features.PatientHealthRecords.DTOs;
using RaphCare.Application.Features.PatientHealthRecords.Queries.GetMyHealthRecordById;
using RaphCare.Application.Features.PatientHealthRecords.Queries.GetMyHealthRecords;

namespace RaphCare.API.Controllers;

/// <summary>Patient-scoped health records (visits / vitals). JWT with <c>patientId</c> claim.</summary>
[Authorize(Policy = "RequirePatient")]
[ApiController]
[Route("api/patient/health-records")]
public class PatientHealthRecordsController(IMediator mediator) : ControllerBase
{
    [HttpGet(Name = "GetMyHealthRecords")]
    [ProducesResponseType(typeof(PagedResult<PatientHealthRecordListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetMyHealthRecordsQuery { PageNumber = pageNumber, PageSize = pageSize }, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpGet("{id:guid}", Name = "GetMyHealthRecordById")]
    [ProducesResponseType(typeof(PatientHealthRecordDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetMyHealthRecordByIdQuery { Id = id }, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }
}
