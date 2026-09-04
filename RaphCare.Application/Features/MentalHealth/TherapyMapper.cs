using RaphCare.Application.Features.MentalHealth.DTOs;
using RaphCare.Domain.MentalHealth;

namespace RaphCare.Application.Features.MentalHealth;

internal static class TherapyMapper
{
    public static TherapySessionDto ToSessionDto(TherapySession s) => new()
    {
        Id = s.Id,
        ClinicId = s.ClinicId,
        PatientId = s.PatientId,
        TherapistUserId = s.TherapistId,
        SessionStart = s.SessionStart,
        SessionEnd = s.SessionEnd,
        SessionType = s.SessionType,
        Status = s.Status,
        Summary = s.Summary,
        IsConfidential = s.IsConfidential,
        Notes = s.TherapyNotes
            .OrderByDescending(n => n.RecordedAt)
            .Select(n => new TherapyNoteDto
            {
                Id = n.Id,
                Notes = n.Notes,
                Category = n.Category,
                IsPrivate = n.IsPrivate,
                RecordedAt = n.RecordedAt
            })
            .ToList(),
        CrisisFlags = s.CrisisFlags
            .OrderByDescending(c => c.FlaggedAt)
            .Select(c => new CrisisFlagDto
            {
                Id = c.Id,
                RiskLevel = c.RiskLevel,
                Description = c.Description,
                FlaggedAt = c.FlaggedAt,
                IsResolved = c.IsResolved,
                ResolvedAt = c.ResolvedAt
            })
            .ToList()
    };

    public static BehavioralCarePlanDto ToCarePlanDto(BehavioralCarePlan p) => new()
    {
        Id = p.Id,
        ClinicId = p.ClinicId,
        PatientId = p.PatientId,
        Title = p.Title,
        Description = p.Description,
        StartDate = p.StartDate,
        EndDate = p.EndDate,
        Status = p.Status,
        Goals = p.Goals
            .OrderBy(g => g.TargetDate)
            .Select(g => new TherapyGoalDto
            {
                Id = g.Id,
                GoalDescription = g.GoalDescription,
                TargetDate = g.TargetDate,
                IsCompleted = g.IsCompleted
            })
            .ToList()
    };
}
