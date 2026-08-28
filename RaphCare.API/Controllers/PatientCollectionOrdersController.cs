using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaphCare.Application.Features.PatientHealthRecords.DTOs;
using RaphCare.Application.Features.PatientHealthRecords.Queries.GetMyCollectionOrders;

namespace RaphCare.API.Controllers;

/// <summary>Pending prescriptions and lab orders for the signed-in patient.</summary>
[Authorize(Policy = "RequirePatient")]
[ApiController]
[Route("api/patient/collection-orders")]
public class PatientCollectionOrdersController(IMediator mediator) : ControllerBase
{
    [HttpGet(Name = "GetMyCollectionOrders")]
    [ProducesResponseType(typeof(PatientCollectionOrdersDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetMyCollectionOrdersQuery(), cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }
}
