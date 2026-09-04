namespace RaphCare.Application.Features.MentalHealth.DTOs;

public sealed class TherapySessionDto
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
    public IReadOnlyList<TherapyNoteDto> Notes { get; set; } = Array.Empty<TherapyNoteDto>();
    public IReadOnlyList<CrisisFlagDto> CrisisFlags { get; set; } = Array.Empty<CrisisFlagDto>();
}

public sealed class TherapyNoteDto
{
    public Guid Id { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsPrivate { get; set; }
    public DateTime RecordedAt { get; set; }
}

public sealed class CrisisFlagDto
{
    public Guid Id { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime FlaggedAt { get; set; }
    public bool IsResolved { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

public sealed class BehavioralCarePlanDto
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public IReadOnlyList<TherapyGoalDto> Goals { get; set; } = Array.Empty<TherapyGoalDto>();
}

public sealed class TherapyGoalDto
{
    public Guid Id { get; set; }
    public string GoalDescription { get; set; } = string.Empty;
    public DateTime TargetDate { get; set; }
    public bool IsCompleted { get; set; }
}

public sealed class MentalHealthNoteDraftDto
{
    public string DraftText { get; set; } = string.Empty;
}
