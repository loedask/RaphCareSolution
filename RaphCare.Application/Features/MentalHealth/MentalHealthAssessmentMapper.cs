using RaphCare.Application.Features.MentalHealth.DTOs;
using RaphCare.Domain.MentalHealth;

namespace RaphCare.Application.Features.MentalHealth;

internal static class MentalHealthAssessmentMapper
{
    public static MentalHealthAssessmentDto ToListDto(MentalHealthAssessment a) => new()
    {
        Id = a.Id,
        ClinicId = a.ClinicId,
        PatientId = a.PatientId,
        AssessmentType = a.AssessmentType,
        AssessedAt = a.ConductedAt,
        TotalScore = a.TotalScore,
        SeverityLevel = a.SeverityLevel,
        Summary = BuildSummary(a)
    };

    public static MentalHealthAssessmentDetailDto ToDetailDto(MentalHealthAssessment a)
    {
        var dto = new MentalHealthAssessmentDetailDto
        {
            Id = a.Id,
            ClinicId = a.ClinicId,
            PatientId = a.PatientId,
            AssessmentType = a.AssessmentType,
            AssessedAt = a.ConductedAt,
            TotalScore = a.TotalScore,
            SeverityLevel = a.SeverityLevel,
            Summary = BuildSummary(a),
            Items = a.Questions
                .OrderBy(q => q.Order)
                .Select(q =>
                {
                    var response = a.Responses.FirstOrDefault(r => r.AssessmentQuestionId == q.Id);
                    return new MentalHealthAssessmentItemDto
                    {
                        Order = q.Order,
                        QuestionText = q.QuestionText,
                        NumericScore = response?.NumericScore,
                        ResponseValue = response?.ResponseValue ?? string.Empty
                    };
                })
                .ToList()
        };
        return dto;
    }

    private static string BuildSummary(MentalHealthAssessment a)
    {
        var score = a.TotalScore is null ? a.SeverityLevel : $"{a.SeverityLevel} (score {a.TotalScore})";
        return $"{a.AssessmentType}: {score}";
    }
}
