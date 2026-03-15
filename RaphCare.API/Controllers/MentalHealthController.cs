using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaphCare.Application.Features.MentalHealth.Queries.GetMentalHealthAssessments;

namespace RaphCare.API.Controllers;

/// <summary>Mental health assessments. Thin API; delegates to MediatR. Roles: Admin, Provider.</summary>
[Authorize(Policy = "RequireProvider")]
[ApiController]
[Route("api/[controller]")]
public class MentalHealthController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>Get mental health assessments. Roles: Admin, Provider.</summary>
    [HttpGet("assessments")]
    public async Task<IActionResult> GetAssessments([FromQuery] Guid clinicId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetMentalHealthAssessmentsQuery { ClinicId = clinicId, PageNumber = pageNumber, PageSize = pageSize }, cancellationToken);
        return Ok(result);
    }
}
