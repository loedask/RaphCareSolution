using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.MentalHealth;

public class TherapyGoal : BaseEntity
{
    public Guid BehavioralCarePlanId { get; set; }

    public string GoalDescription { get; set; } = null!;
    public DateTime TargetDate { get; set; }
    public bool IsCompleted { get; set; }

    public BehavioralCarePlan BehavioralCarePlan { get; set; } = null!;
    public ICollection<TherapyGoalProgress> ProgressEntries { get; set; } = new List<TherapyGoalProgress>();
}

