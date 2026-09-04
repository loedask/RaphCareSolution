using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Features.MentalHealth.Commands.AddTherapyNote;
using RaphCare.Application.Features.MentalHealth.Commands.CompleteTherapyGoal;
using RaphCare.Application.Features.MentalHealth.Commands.CreateBehavioralCarePlan;
using RaphCare.Application.Features.MentalHealth.Commands.CreateMentalHealthAssessment;
using RaphCare.Application.Features.MentalHealth.Commands.CreateTherapySession;
using RaphCare.Application.Features.MentalHealth.Commands.DraftMentalHealthNote;
using RaphCare.Application.Features.MentalHealth.Commands.ResolveCrisisFlag;
using RaphCare.Application.Features.MentalHealth.DTOs;
using RaphCare.Application.Features.MentalHealth.Queries.GetBehavioralCarePlans;
using RaphCare.Application.Features.MentalHealth.Queries.GetMentalHealthAssessmentById;
using RaphCare.Application.Features.MentalHealth.Queries.GetMentalHealthAssessments;
using RaphCare.Application.Features.MentalHealth.Queries.GetMentalHealthInstrument;
using RaphCare.Application.Features.MentalHealth.Queries.GetTherapySessions;

namespace RaphCare.API.Controllers;

/// <summary>Staff mental health assessments, therapy, and care plans. Thin API; delegates to MediatR.</summary>
[Authorize(Policy = "RequireProvider")]
[ApiController]
[Route("api/[controller]")]
public class MentalHealthController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>List mental health assessments for a clinic (optional patient filter).</summary>
    [HttpGet("assessments", Name = "GetMentalHealthAssessments")]
    [ProducesResponseType(typeof(PagedResult<MentalHealthAssessmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAssessments(
        [FromQuery] Guid clinicId,
        [FromQuery] Guid? patientId = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetMentalHealthAssessmentsQuery
            {
                ClinicId = clinicId,
                PatientId = patientId,
                PageNumber = pageNumber,
                PageSize = pageSize
            },
            cancellationToken);
        return Ok(result);
    }

    /// <summary>Get one assessment with item answers.</summary>
    [HttpGet("assessments/{assessmentId:guid}", Name = "GetMentalHealthAssessmentById")]
    [ProducesResponseType(typeof(MentalHealthAssessmentDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAssessmentById(
        Guid assessmentId,
        [FromQuery] Guid clinicId,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetMentalHealthAssessmentByIdQuery
            {
                ClinicId = clinicId,
                AssessmentId = assessmentId
            },
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Instrument definition for the staff recording form (PHQ-9 / GAD-7).</summary>
    [HttpGet("instruments/{assessmentType}", Name = "GetMentalHealthInstrument")]
    [ProducesResponseType(typeof(MentalHealthInstrumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInstrument(
        string assessmentType,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetMentalHealthInstrumentQuery { AssessmentType = assessmentType },
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Record a staff mental health assessment (PHQ-9 / GAD-7).</summary>
    [HttpPost("assessments", Name = "CreateMentalHealthAssessment")]
    [ProducesResponseType(typeof(MentalHealthAssessmentDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateAssessment(
        [FromBody] CreateMentalHealthAssessmentCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(command, cancellationToken);
        if (result is null)
            return NotFound();

        return CreatedAtRoute(
            "GetMentalHealthAssessmentById",
            new { assessmentId = result.Id, clinicId = result.ClinicId },
            result);
    }

    /// <summary>List therapy sessions for a clinic (optional patient filter).</summary>
    [HttpGet("therapy-sessions", Name = "GetTherapySessions")]
    [ProducesResponseType(typeof(PagedResult<TherapySessionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTherapySessions(
        [FromQuery] Guid clinicId,
        [FromQuery] Guid? patientId = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetTherapySessionsQuery
            {
                ClinicId = clinicId,
                PatientId = patientId,
                PageNumber = pageNumber,
                PageSize = pageSize
            },
            cancellationToken);
        return Ok(result);
    }

    /// <summary>Log a therapy session for a patient.</summary>
    [HttpPost("therapy-sessions", Name = "CreateTherapySession")]
    [ProducesResponseType(typeof(TherapySessionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateTherapySession(
        [FromBody] CreateTherapySessionCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result is null ? NotFound() : StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>Add a therapy note (optional crisis flag) to a session.</summary>
    [HttpPost("therapy-sessions/{sessionId:guid}/notes", Name = "AddTherapyNote")]
    [ProducesResponseType(typeof(TherapyNoteDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddTherapyNote(
        Guid sessionId,
        [FromBody] AddTherapyNoteCommand command,
        CancellationToken cancellationToken = default)
    {
        command.SessionId = sessionId;
        var result = await _mediator.Send(command, cancellationToken);
        return result is null ? NotFound() : StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>Resolve a crisis flag on a therapy session.</summary>
    [HttpPost("therapy-sessions/{sessionId:guid}/crisis-flags/{crisisFlagId:guid}/resolve", Name = "ResolveCrisisFlag")]
    [ProducesResponseType(typeof(CrisisFlagDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResolveCrisisFlag(
        Guid sessionId,
        Guid crisisFlagId,
        [FromBody] ResolveCrisisFlagCommand command,
        CancellationToken cancellationToken = default)
    {
        command.SessionId = sessionId;
        command.CrisisFlagId = crisisFlagId;
        var result = await _mediator.Send(command, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>List behavioral care plans for a clinic (optional patient filter).</summary>
    [HttpGet("care-plans", Name = "GetBehavioralCarePlans")]
    [ProducesResponseType(typeof(PagedResult<BehavioralCarePlanDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBehavioralCarePlans(
        [FromQuery] Guid clinicId,
        [FromQuery] Guid? patientId = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetBehavioralCarePlansQuery
            {
                ClinicId = clinicId,
                PatientId = patientId,
                PageNumber = pageNumber,
                PageSize = pageSize
            },
            cancellationToken);
        return Ok(result);
    }

    /// <summary>Create a behavioral care plan with goals.</summary>
    [HttpPost("care-plans", Name = "CreateBehavioralCarePlan")]
    [ProducesResponseType(typeof(BehavioralCarePlanDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateBehavioralCarePlan(
        [FromBody] CreateBehavioralCarePlanCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result is null ? NotFound() : StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>Mark a therapy goal completed or reopen it.</summary>
    [HttpPost("care-plans/{carePlanId:guid}/goals/{goalId:guid}/complete", Name = "CompleteTherapyGoal")]
    [ProducesResponseType(typeof(TherapyGoalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteTherapyGoal(
        Guid carePlanId,
        Guid goalId,
        [FromBody] CompleteTherapyGoalCommand command,
        CancellationToken cancellationToken = default)
    {
        command.CarePlanId = carePlanId;
        command.GoalId = goalId;
        var result = await _mediator.Send(command, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Draft an AI mental health note from assessment and mood scores (staff edits before save).</summary>
    [HttpPost("notes/draft", Name = "DraftMentalHealthNote")]
    [ProducesResponseType(typeof(MentalHealthNoteDraftDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DraftMentalHealthNote(
        [FromBody] DraftMentalHealthNoteCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
