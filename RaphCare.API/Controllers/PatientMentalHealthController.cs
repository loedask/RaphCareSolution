using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaphCare.API.App.Contracts;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Features.MentalHealth.DTOs;
using RaphCare.Application.Features.PatientMentalHealth.Commands.LogMyPatientMoodCheckIn;
using RaphCare.Application.Features.PatientMentalHealth.Commands.SubmitMyPatientMentalHealthAssessment;
using RaphCare.Application.Features.PatientMentalHealth.DTOs;
using RaphCare.Application.Features.PatientMentalHealth.Queries.GetMyPatientMentalHealthAssessments;
using RaphCare.Application.Features.PatientMentalHealth.Queries.GetMyPatientMentalHealthContent;
using RaphCare.Application.Features.PatientMentalHealth.Queries.GetMyPatientMentalHealthInstrument;

namespace RaphCare.API.Controllers;

/// <summary>Patient mental health hub (concept <c>/mental-health</c>): content, mood, self-assessments.</summary>
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

    /// <summary>Instrument definition for patient self-assessment (PHQ-9 / GAD-7).</summary>
    [HttpGet("instruments/{assessmentType}", Name = "GetMyPatientMentalHealthInstrument")]
    [ProducesResponseType(typeof(MentalHealthInstrumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInstrument(
        string assessmentType,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetMyPatientMentalHealthInstrumentQuery { AssessmentType = assessmentType },
            cancellationToken).ConfigureAwait(false);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>List the current patient's mental health assessments (optional clinic filter).</summary>
    [HttpGet("assessments", Name = "GetMyPatientMentalHealthAssessments")]
    [ProducesResponseType(typeof(PagedResult<MentalHealthAssessmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAssessments(
        [FromQuery] Guid? clinicId = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetMyPatientMentalHealthAssessmentsQuery
            {
                ClinicId = clinicId,
                PageNumber = pageNumber,
                PageSize = pageSize
            },
            cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>Submit a patient self-assessment (PHQ-9 / GAD-7).</summary>
    [HttpPost("assessments", Name = "SubmitMyPatientMentalHealthAssessment")]
    [ProducesResponseType(typeof(MentalHealthAssessmentDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubmitAssessment(
        [FromBody] SubmitMyPatientMentalHealthAssessmentCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return result is null
            ? NotFound()
            : StatusCode(StatusCodes.Status201Created, result);
    }
}
