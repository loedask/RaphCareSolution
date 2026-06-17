using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.MentalHealth;

public class MoodLog : BaseEntity
{
    public Guid PatientId { get; set; }

    public DateTime LoggedAt { get; set; }
    public int MoodScore { get; set; }
    public string? Notes { get; set; }
    public bool IsFlagged { get; set; }
}

