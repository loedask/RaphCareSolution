using RaphCare.Domain.Common;

namespace RaphCare.Domain.Telemedicine;

/// <summary>
/// Optional intake questionnaire before a telemedicine session (triage and AI routing).
/// </summary>
public class PreVisitQuestionnaire : BaseEntity
{
    public Guid TeleSessionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }

    public TeleSession TeleSession { get; set; } = null!;
    public ICollection<QuestionnaireResponse> QuestionnaireResponses { get; set; } = new List<QuestionnaireResponse>();
}
