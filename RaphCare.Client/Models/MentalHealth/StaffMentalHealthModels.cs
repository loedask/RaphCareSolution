namespace RaphCare.Client.Models.MentalHealth;

public sealed class MentalHealthAssessmentListItem
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    public string AssessmentType { get; set; } = string.Empty;
    public DateTime AssessedAt { get; set; }
    public decimal? TotalScore { get; set; }
    public string SeverityLevel { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
}

public sealed class MentalHealthAssessmentDetail
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    public string AssessmentType { get; set; } = string.Empty;
    public DateTime AssessedAt { get; set; }
    public decimal? TotalScore { get; set; }
    public string SeverityLevel { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public IReadOnlyList<MentalHealthAssessmentItem> Items { get; set; } = Array.Empty<MentalHealthAssessmentItem>();
}

public sealed class MentalHealthAssessmentItem
{
    public int Order { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int? NumericScore { get; set; }
    public string ResponseValue { get; set; } = string.Empty;
}

public sealed class MentalHealthInstrument
{
    public string AssessmentType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public IReadOnlyList<MentalHealthInstrumentQuestion> Questions { get; set; } =
        Array.Empty<MentalHealthInstrumentQuestion>();
    public IReadOnlyList<MentalHealthInstrumentOption> Options { get; set; } =
        Array.Empty<MentalHealthInstrumentOption>();
}

public sealed class MentalHealthInstrumentQuestion
{
    public int Order { get; set; }
    public string QuestionText { get; set; } = string.Empty;
}

public sealed class MentalHealthInstrumentOption
{
    public int NumericScore { get; set; }
    public string Label { get; set; } = string.Empty;
}

public sealed class CreateMentalHealthAssessmentRequest
{
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    public string AssessmentType { get; set; } = "PHQ-9";
    public IReadOnlyList<CreateMentalHealthAssessmentAnswerRequest> Answers { get; set; } =
        Array.Empty<CreateMentalHealthAssessmentAnswerRequest>();
}

public sealed class CreateMentalHealthAssessmentAnswerRequest
{
    public int Order { get; set; }
    public int NumericScore { get; set; }
}

public sealed class SubmitPatientMentalHealthAssessmentRequest
{
    public Guid ClinicId { get; set; }
    public string AssessmentType { get; set; } = "PHQ-9";
    public IReadOnlyList<CreateMentalHealthAssessmentAnswerRequest> Answers { get; set; } =
        Array.Empty<CreateMentalHealthAssessmentAnswerRequest>();
}

public sealed class TherapySessionListItem
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    public Guid TherapistUserId { get; set; }
    public DateTime SessionStart { get; set; }
    public DateTime? SessionEnd { get; set; }
    public string SessionType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public bool IsConfidential { get; set; }
    public IReadOnlyList<TherapyNoteItem> Notes { get; set; } = Array.Empty<TherapyNoteItem>();
    public IReadOnlyList<CrisisFlagItem> CrisisFlags { get; set; } = Array.Empty<CrisisFlagItem>();
}

public sealed class TherapyNoteItem
{
    public Guid Id { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsPrivate { get; set; }
    public DateTime RecordedAt { get; set; }
}

public sealed class CrisisFlagItem
{
    public Guid Id { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime FlaggedAt { get; set; }
    public bool IsResolved { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

public sealed class CreateTherapySessionRequest
{
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    public string SessionType { get; set; } = "Individual";
    public string? Summary { get; set; }
    public bool IsConfidential { get; set; } = true;
}

public sealed class AddTherapyNoteRequest
{
    public Guid ClinicId { get; set; }
    public Guid SessionId { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string Category { get; set; } = "Progress";
    public bool IsPrivate { get; set; } = true;
    public string? CrisisRiskLevel { get; set; }
    public string? CrisisDescription { get; set; }
}

public sealed class ResolveCrisisFlagRequest
{
    public Guid ClinicId { get; set; }
    public Guid SessionId { get; set; }
    public Guid CrisisFlagId { get; set; }
}

public sealed class BehavioralCarePlanItem
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public IReadOnlyList<TherapyGoalItem> Goals { get; set; } = Array.Empty<TherapyGoalItem>();
}

public sealed class TherapyGoalItem
{
    public Guid Id { get; set; }
    public string GoalDescription { get; set; } = string.Empty;
    public DateTime TargetDate { get; set; }
    public bool IsCompleted { get; set; }
}

public sealed class CreateBehavioralCarePlanRequest
{
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IReadOnlyList<CreateBehavioralCarePlanGoalRequest> Goals { get; set; } =
        Array.Empty<CreateBehavioralCarePlanGoalRequest>();
}

public sealed class CreateBehavioralCarePlanGoalRequest
{
    public string GoalDescription { get; set; } = string.Empty;
    public DateTime TargetDate { get; set; }
}

public sealed class CompleteTherapyGoalRequest
{
    public Guid ClinicId { get; set; }
    public Guid CarePlanId { get; set; }
    public Guid GoalId { get; set; }
    public bool IsCompleted { get; set; } = true;
}

public sealed class DraftMentalHealthNoteRequest
{
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
}

public sealed class MentalHealthNoteDraft
{
    public string DraftText { get; set; } = string.Empty;
}
