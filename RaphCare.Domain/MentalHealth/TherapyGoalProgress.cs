using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.MentalHealth;

public class TherapyGoalProgress : BaseEntity
{
    public Guid TherapyGoalId { get; set; }

    public DateTime RecordedAt { get; set; }
    public string Notes { get; set; } = null!;
    public decimal ProgressPercentage { get; set; }

    public TherapyGoal TherapyGoal { get; set; } = null!;
}

