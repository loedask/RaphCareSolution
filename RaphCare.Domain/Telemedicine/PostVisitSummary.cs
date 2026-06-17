using RaphCare.Domain.Common;

namespace RaphCare.Domain.Telemedicine;

/// <summary>
/// AI or provider-generated summary after a telemedicine session.
/// </summary>
public class PostVisitSummary : BaseEntity
{
    public Guid TeleSessionId { get; set; }
    public string SummaryText { get; set; } = string.Empty;
    public bool GeneratedByAI { get; set; }
    public DateTime GeneratedAt { get; set; }
    public Guid? GeneratedByUserId { get; set; }

    public TeleSession TeleSession { get; set; } = null!;
}
