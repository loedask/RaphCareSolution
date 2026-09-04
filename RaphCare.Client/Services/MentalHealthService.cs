using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Api;
using RaphCare.Client.Models.MentalHealth;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client.Services;

public sealed class MentalHealthService(HttpClient httpClient) : BaseHttpService(httpClient), IMentalHealthService
{
    public async Task<Response<PagedApiResult<MentalHealthAssessmentListItem>>> GetAssessmentsAsync(
        Guid clinicId,
        Guid? patientId = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query =
            $"api/MentalHealth/assessments?clinicId={clinicId:D}&pageNumber={pageNumber}&pageSize={pageSize}";
        if (patientId is Guid pid)
            query += $"&patientId={pid:D}";

        var result = await GetAsync<PagedApiResult<AssessmentListDto>>(query, cancellationToken)
            .ConfigureAwait(false);
        if (!result.IsSuccess || result.Data is null)
            return Response<PagedApiResult<MentalHealthAssessmentListItem>>.Failure(
                result.ErrorMessage ?? "Could not load assessments.",
                result.StatusCode);

        return Response<PagedApiResult<MentalHealthAssessmentListItem>>.Success(
            new PagedApiResult<MentalHealthAssessmentListItem>
            {
                Items = result.Data.Items?.Select(MapListItem).ToList() ?? [],
                TotalCount = result.Data.TotalCount,
                PageNumber = result.Data.PageNumber,
                PageSize = result.Data.PageSize
            });
    }

    public async Task<Response<MentalHealthAssessmentDetail>> GetAssessmentByIdAsync(
        Guid clinicId,
        Guid assessmentId,
        CancellationToken cancellationToken = default)
    {
        var result = await GetAsync<AssessmentDetailDto>(
                $"api/MentalHealth/assessments/{assessmentId:D}?clinicId={clinicId:D}",
                cancellationToken)
            .ConfigureAwait(false);
        if (!result.IsSuccess || result.Data is null)
            return Response<MentalHealthAssessmentDetail>.Failure(
                result.ErrorMessage ?? "Assessment not found.",
                result.StatusCode);
        return Response<MentalHealthAssessmentDetail>.Success(MapDetail(result.Data));
    }

    public async Task<Response<MentalHealthInstrument>> GetInstrumentAsync(
        string assessmentType = "PHQ-9",
        CancellationToken cancellationToken = default)
    {
        var encoded = Uri.EscapeDataString(assessmentType);
        var result = await GetAsync<InstrumentDto>(
                $"api/MentalHealth/instruments/{encoded}",
                cancellationToken)
            .ConfigureAwait(false);
        if (!result.IsSuccess || result.Data is null)
            return Response<MentalHealthInstrument>.Failure(
                result.ErrorMessage ?? "Instrument not found.",
                result.StatusCode);
        return Response<MentalHealthInstrument>.Success(MapInstrument(result.Data));
    }

    public async Task<Response<MentalHealthAssessmentDetail>> CreateAssessmentAsync(
        CreateMentalHealthAssessmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var body = new
        {
            clinicId = request.ClinicId,
            patientId = request.PatientId,
            assessmentType = request.AssessmentType,
            answers = request.Answers.Select(a => new { order = a.Order, numericScore = a.NumericScore })
        };

        var result = await PostAsync<AssessmentDetailDto>("api/MentalHealth/assessments", body, cancellationToken)
            .ConfigureAwait(false);
        if (!result.IsSuccess || result.Data is null)
            return Response<MentalHealthAssessmentDetail>.Failure(
                result.ErrorMessage ?? "Could not save the assessment.",
                result.StatusCode);
        return Response<MentalHealthAssessmentDetail>.Success(MapDetail(result.Data));
    }

    public async Task<Response<PagedApiResult<TherapySessionListItem>>> GetTherapySessionsAsync(
        Guid clinicId,
        Guid? patientId = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query =
            $"api/MentalHealth/therapy-sessions?clinicId={clinicId:D}&pageNumber={pageNumber}&pageSize={pageSize}";
        if (patientId is Guid pid)
            query += $"&patientId={pid:D}";

        var result = await GetAsync<PagedApiResult<TherapySessionDto>>(query, cancellationToken)
            .ConfigureAwait(false);
        if (!result.IsSuccess || result.Data is null)
            return Response<PagedApiResult<TherapySessionListItem>>.Failure(
                result.ErrorMessage ?? "Could not load therapy sessions.",
                result.StatusCode);

        return Response<PagedApiResult<TherapySessionListItem>>.Success(
            new PagedApiResult<TherapySessionListItem>
            {
                Items = result.Data.Items?.Select(MapSession).ToList() ?? [],
                TotalCount = result.Data.TotalCount,
                PageNumber = result.Data.PageNumber,
                PageSize = result.Data.PageSize
            });
    }

    public async Task<Response<TherapySessionListItem>> CreateTherapySessionAsync(
        CreateTherapySessionRequest request,
        CancellationToken cancellationToken = default)
    {
        var body = new
        {
            clinicId = request.ClinicId,
            patientId = request.PatientId,
            sessionType = request.SessionType,
            summary = request.Summary,
            isConfidential = request.IsConfidential
        };

        var result = await PostAsync<TherapySessionDto>("api/MentalHealth/therapy-sessions", body, cancellationToken)
            .ConfigureAwait(false);
        if (!result.IsSuccess || result.Data is null)
            return Response<TherapySessionListItem>.Failure(
                result.ErrorMessage ?? "Could not log the therapy session.",
                result.StatusCode);
        return Response<TherapySessionListItem>.Success(MapSession(result.Data));
    }

    public async Task<Response<TherapyNoteItem>> AddTherapyNoteAsync(
        AddTherapyNoteRequest request,
        CancellationToken cancellationToken = default)
    {
        var body = new
        {
            clinicId = request.ClinicId,
            sessionId = request.SessionId,
            notes = request.Notes,
            category = request.Category,
            isPrivate = request.IsPrivate,
            crisisRiskLevel = request.CrisisRiskLevel,
            crisisDescription = request.CrisisDescription
        };

        var result = await PostAsync<TherapyNoteDto>(
                $"api/MentalHealth/therapy-sessions/{request.SessionId:D}/notes",
                body,
                cancellationToken)
            .ConfigureAwait(false);
        if (!result.IsSuccess || result.Data is null)
            return Response<TherapyNoteItem>.Failure(
                result.ErrorMessage ?? "Could not add the therapy note.",
                result.StatusCode);
        return Response<TherapyNoteItem>.Success(MapNote(result.Data));
    }

    public async Task<Response<CrisisFlagItem>> ResolveCrisisFlagAsync(
        ResolveCrisisFlagRequest request,
        CancellationToken cancellationToken = default)
    {
        var body = new { clinicId = request.ClinicId, sessionId = request.SessionId, crisisFlagId = request.CrisisFlagId };
        var result = await PostAsync<CrisisFlagDto>(
                $"api/MentalHealth/therapy-sessions/{request.SessionId:D}/crisis-flags/{request.CrisisFlagId:D}/resolve",
                body,
                cancellationToken)
            .ConfigureAwait(false);
        if (!result.IsSuccess || result.Data is null)
            return Response<CrisisFlagItem>.Failure(
                result.ErrorMessage ?? "Could not resolve the crisis flag.",
                result.StatusCode);
        return Response<CrisisFlagItem>.Success(MapCrisis(result.Data));
    }

    public async Task<Response<PagedApiResult<BehavioralCarePlanItem>>> GetBehavioralCarePlansAsync(
        Guid clinicId,
        Guid? patientId = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query =
            $"api/MentalHealth/care-plans?clinicId={clinicId:D}&pageNumber={pageNumber}&pageSize={pageSize}";
        if (patientId is Guid pid)
            query += $"&patientId={pid:D}";

        var result = await GetAsync<PagedApiResult<CarePlanDto>>(query, cancellationToken)
            .ConfigureAwait(false);
        if (!result.IsSuccess || result.Data is null)
            return Response<PagedApiResult<BehavioralCarePlanItem>>.Failure(
                result.ErrorMessage ?? "Could not load care plans.",
                result.StatusCode);

        return Response<PagedApiResult<BehavioralCarePlanItem>>.Success(
            new PagedApiResult<BehavioralCarePlanItem>
            {
                Items = result.Data.Items?.Select(MapCarePlan).ToList() ?? [],
                TotalCount = result.Data.TotalCount,
                PageNumber = result.Data.PageNumber,
                PageSize = result.Data.PageSize
            });
    }

    public async Task<Response<BehavioralCarePlanItem>> CreateBehavioralCarePlanAsync(
        CreateBehavioralCarePlanRequest request,
        CancellationToken cancellationToken = default)
    {
        var body = new
        {
            clinicId = request.ClinicId,
            patientId = request.PatientId,
            title = request.Title,
            description = request.Description,
            goals = request.Goals.Select(g => new { goalDescription = g.GoalDescription, targetDate = g.TargetDate })
        };

        var result = await PostAsync<CarePlanDto>("api/MentalHealth/care-plans", body, cancellationToken)
            .ConfigureAwait(false);
        if (!result.IsSuccess || result.Data is null)
            return Response<BehavioralCarePlanItem>.Failure(
                result.ErrorMessage ?? "Could not create the care plan.",
                result.StatusCode);
        return Response<BehavioralCarePlanItem>.Success(MapCarePlan(result.Data));
    }

    public async Task<Response<TherapyGoalItem>> CompleteTherapyGoalAsync(
        CompleteTherapyGoalRequest request,
        CancellationToken cancellationToken = default)
    {
        var body = new
        {
            clinicId = request.ClinicId,
            carePlanId = request.CarePlanId,
            goalId = request.GoalId,
            isCompleted = request.IsCompleted
        };

        var result = await PostAsync<TherapyGoalDto>(
                $"api/MentalHealth/care-plans/{request.CarePlanId:D}/goals/{request.GoalId:D}/complete",
                body,
                cancellationToken)
            .ConfigureAwait(false);
        if (!result.IsSuccess || result.Data is null)
            return Response<TherapyGoalItem>.Failure(
                result.ErrorMessage ?? "Could not update the goal.",
                result.StatusCode);
        return Response<TherapyGoalItem>.Success(MapGoal(result.Data));
    }

    public async Task<Response<MentalHealthNoteDraft>> DraftMentalHealthNoteAsync(
        DraftMentalHealthNoteRequest request,
        CancellationToken cancellationToken = default)
    {
        var body = new { clinicId = request.ClinicId, patientId = request.PatientId };
        var result = await PostAsync<NoteDraftDto>("api/MentalHealth/notes/draft", body, cancellationToken)
            .ConfigureAwait(false);
        if (!result.IsSuccess || result.Data is null)
            return Response<MentalHealthNoteDraft>.Failure(
                result.ErrorMessage ?? "Could not draft the note.",
                result.StatusCode);
        return Response<MentalHealthNoteDraft>.Success(
            new MentalHealthNoteDraft { DraftText = result.Data.DraftText ?? string.Empty });
    }

    private static MentalHealthAssessmentListItem MapListItem(AssessmentListDto d) => new()
    {
        Id = d.Id,
        ClinicId = d.ClinicId,
        PatientId = d.PatientId,
        AssessmentType = d.AssessmentType ?? string.Empty,
        AssessedAt = d.AssessedAt,
        TotalScore = d.TotalScore,
        SeverityLevel = d.SeverityLevel ?? string.Empty,
        Summary = d.Summary ?? string.Empty
    };

    private static MentalHealthAssessmentDetail MapDetail(AssessmentDetailDto d) => new()
    {
        Id = d.Id,
        ClinicId = d.ClinicId,
        PatientId = d.PatientId,
        AssessmentType = d.AssessmentType ?? string.Empty,
        AssessedAt = d.AssessedAt,
        TotalScore = d.TotalScore,
        SeverityLevel = d.SeverityLevel ?? string.Empty,
        Summary = d.Summary ?? string.Empty,
        Items = d.Items?.Select(i => new MentalHealthAssessmentItem
        {
            Order = i.Order,
            QuestionText = i.QuestionText ?? string.Empty,
            NumericScore = i.NumericScore,
            ResponseValue = i.ResponseValue ?? string.Empty
        }).ToList() ?? []
    };

    private static MentalHealthInstrument MapInstrument(InstrumentDto d) => new()
    {
        AssessmentType = d.AssessmentType ?? string.Empty,
        Title = d.Title ?? string.Empty,
        Instructions = d.Instructions ?? string.Empty,
        Questions = d.Questions?.Select(q => new MentalHealthInstrumentQuestion
        {
            Order = q.Order,
            QuestionText = q.QuestionText ?? string.Empty
        }).ToList() ?? [],
        Options = d.Options?.Select(o => new MentalHealthInstrumentOption
        {
            NumericScore = o.NumericScore,
            Label = o.Label ?? string.Empty
        }).ToList() ?? []
    };

    private static TherapySessionListItem MapSession(TherapySessionDto d) => new()
    {
        Id = d.Id,
        ClinicId = d.ClinicId,
        PatientId = d.PatientId,
        TherapistUserId = d.TherapistUserId,
        SessionStart = d.SessionStart,
        SessionEnd = d.SessionEnd,
        SessionType = d.SessionType ?? string.Empty,
        Status = d.Status ?? string.Empty,
        Summary = d.Summary,
        IsConfidential = d.IsConfidential,
        Notes = d.Notes?.Select(MapNote).ToList() ?? [],
        CrisisFlags = d.CrisisFlags?.Select(MapCrisis).ToList() ?? []
    };

    private static TherapyNoteItem MapNote(TherapyNoteDto d) => new()
    {
        Id = d.Id,
        Notes = d.Notes ?? string.Empty,
        Category = d.Category ?? string.Empty,
        IsPrivate = d.IsPrivate,
        RecordedAt = d.RecordedAt
    };

    private static CrisisFlagItem MapCrisis(CrisisFlagDto d) => new()
    {
        Id = d.Id,
        RiskLevel = d.RiskLevel ?? string.Empty,
        Description = d.Description ?? string.Empty,
        FlaggedAt = d.FlaggedAt,
        IsResolved = d.IsResolved,
        ResolvedAt = d.ResolvedAt
    };

    private static BehavioralCarePlanItem MapCarePlan(CarePlanDto d) => new()
    {
        Id = d.Id,
        ClinicId = d.ClinicId,
        PatientId = d.PatientId,
        Title = d.Title ?? string.Empty,
        Description = d.Description ?? string.Empty,
        StartDate = d.StartDate,
        EndDate = d.EndDate,
        Status = d.Status ?? string.Empty,
        Goals = d.Goals?.Select(MapGoal).ToList() ?? []
    };

    private static TherapyGoalItem MapGoal(TherapyGoalDto d) => new()
    {
        Id = d.Id,
        GoalDescription = d.GoalDescription ?? string.Empty,
        TargetDate = d.TargetDate,
        IsCompleted = d.IsCompleted
    };

    private class AssessmentListDto
    {
        public Guid Id { get; set; }
        public Guid ClinicId { get; set; }
        public Guid PatientId { get; set; }
        public string? AssessmentType { get; set; }
        public DateTime AssessedAt { get; set; }
        public decimal? TotalScore { get; set; }
        public string? SeverityLevel { get; set; }
        public string? Summary { get; set; }
    }

    private sealed class AssessmentDetailDto : AssessmentListDto
    {
        public List<AssessmentItemDto>? Items { get; set; }
    }

    private sealed class AssessmentItemDto
    {
        public int Order { get; set; }
        public string? QuestionText { get; set; }
        public int? NumericScore { get; set; }
        public string? ResponseValue { get; set; }
    }

    private sealed class InstrumentDto
    {
        public string? AssessmentType { get; set; }
        public string? Title { get; set; }
        public string? Instructions { get; set; }
        public List<InstrumentQuestionDto>? Questions { get; set; }
        public List<InstrumentOptionDto>? Options { get; set; }
    }

    private sealed class InstrumentQuestionDto
    {
        public int Order { get; set; }
        public string? QuestionText { get; set; }
    }

    private sealed class InstrumentOptionDto
    {
        public int NumericScore { get; set; }
        public string? Label { get; set; }
    }

    private sealed class TherapySessionDto
    {
        public Guid Id { get; set; }
        public Guid ClinicId { get; set; }
        public Guid PatientId { get; set; }
        public Guid TherapistUserId { get; set; }
        public DateTime SessionStart { get; set; }
        public DateTime? SessionEnd { get; set; }
        public string? SessionType { get; set; }
        public string? Status { get; set; }
        public string? Summary { get; set; }
        public bool IsConfidential { get; set; }
        public List<TherapyNoteDto>? Notes { get; set; }
        public List<CrisisFlagDto>? CrisisFlags { get; set; }
    }

    private sealed class TherapyNoteDto
    {
        public Guid Id { get; set; }
        public string? Notes { get; set; }
        public string? Category { get; set; }
        public bool IsPrivate { get; set; }
        public DateTime RecordedAt { get; set; }
    }

    private sealed class CrisisFlagDto
    {
        public Guid Id { get; set; }
        public string? RiskLevel { get; set; }
        public string? Description { get; set; }
        public DateTime FlaggedAt { get; set; }
        public bool IsResolved { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }

    private sealed class CarePlanDto
    {
        public Guid Id { get; set; }
        public Guid ClinicId { get; set; }
        public Guid PatientId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Status { get; set; }
        public List<TherapyGoalDto>? Goals { get; set; }
    }

    private sealed class TherapyGoalDto
    {
        public Guid Id { get; set; }
        public string? GoalDescription { get; set; }
        public DateTime TargetDate { get; set; }
        public bool IsCompleted { get; set; }
    }

    private sealed class NoteDraftDto
    {
        public string? DraftText { get; set; }
    }
}
