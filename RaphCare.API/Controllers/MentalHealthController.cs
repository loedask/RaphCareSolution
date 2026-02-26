using MediatR;
using Microsoft.AspNetCore.Mvc;
using RaphCare.Application.Features.MentalHealth.Queries.GetMentalHealthAssessments;

namespace RaphCare.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MentalHealthController : ControllerBase
{
    private readonly IMediator _mediator;

    public MentalHealthController(IMediator mediator) { _mediator = mediator; }

    /// <summary>Get mental health assessments. Roles: Admin, Provider.</summary>
    [HttpGet("assessments")]
    public async Task<IActionResult> GetAssessments([FromQuery] Guid clinicId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetMentalHealthAssessmentsQuery { ClinicId = clinicId, PageNumber = pageNumber, PageSize = pageSize }, cancellationToken);
        return Ok(result);
    }
}
