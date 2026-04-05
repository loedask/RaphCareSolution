using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaphCare.API.App.Contracts;
using RaphCare.Application.Features.PatientMentalHealth.Commands.LogMyPatientMoodCheckIn;
using RaphCare.Application.Features.PatientMentalHealth.DTOs;
using RaphCare.Application.Features.PatientMentalHealth.Queries.GetMyPatientMentalHealthContent;

namespace RaphCare.API.Controllers;

/// <summary>Patient mental health hub (concept <c>/mental-health</c>): configurable copy + mood check-in.</summary>
[Authorize(Policy = "RequirePatient")]
[ApiController]
[Route("api/patient/mental-health")]
public sealed class PatientMentalHealthController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>Wellness insight and disclaimer strings from host configuration (no PHI).</summary>
    [HttpGet("content", Name = "GetMyPatientMentalHealthContent")]
    [ProducesResponseType(typeof(PatientMentalHealthContentDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetContent(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyPatientMentalHealthContentQuery(), cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>Records a mood check-in for the current patient.</summary>
    [HttpPost("mood-checkin", Name = "LogMyPatientMoodCheckIn")]
    [ProducesResponseType(typeof(CreatedGuidResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> LogMoodCheckIn([FromBody] LogMyPatientMoodCheckInCommand body, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(body, cancellationToken).ConfigureAwait(false);
        return StatusCode(StatusCodes.Status201Created, new CreatedGuidResponse { Id = id });
    }
}
