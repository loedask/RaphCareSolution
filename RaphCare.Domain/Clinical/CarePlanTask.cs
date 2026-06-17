using RaphCare.Domain.Common;

namespace RaphCare.Domain.Clinical;

/// <summary>
/// Task within a care plan.
/// </summary>
public class CarePlanTask : BaseEntity
{
    public Guid CarePlanId { get; set; }
    public string TaskDescription { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }

    public CarePlan CarePlan { get; set; } = null!;
}
