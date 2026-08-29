using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Application.Features.Organization.Queries.GetCollectionDisplay;

namespace RaphCare.API.Controllers;

/// <summary>Public waiting-room board. Pickup codes only. Does not require X-Clinic-Id or sign-in.</summary>
[AllowAnonymous]
[ApiController]
[Route("api/display")]
public sealed class CollectionDisplayController(IMediator mediator) : ControllerBase
{
    [HttpGet("collection/{token}", Name = "GetCollectionDisplay")]
    [ProducesResponseType(typeof(CollectionDisplayBoardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCollection(string token, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCollectionDisplayQuery { Token = token }, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
