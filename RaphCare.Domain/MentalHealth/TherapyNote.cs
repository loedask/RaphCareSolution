using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.MentalHealth;

public class TherapyNote : BaseEntity
{
    public Guid TherapySessionId { get; set; }

    public string Notes { get; set; } = null!;
    public string Category { get; set; } = null!;
    public bool IsPrivate { get; set; }
    public DateTime RecordedAt { get; set; }

    public TherapySession TherapySession { get; set; } = null!;
}

