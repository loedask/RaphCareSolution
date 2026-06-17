using RaphCare.Domain.Common;

namespace RaphCare.Domain.Telemedicine;

/// <summary>
/// Single question-answer pair within a pre-visit questionnaire.
/// </summary>
public class QuestionnaireResponse : BaseEntity
{
    public Guid PreVisitQuestionnaireId { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public DateTime AnsweredAt { get; set; }

    public PreVisitQuestionnaire PreVisitQuestionnaire { get; set; } = null!;
}
