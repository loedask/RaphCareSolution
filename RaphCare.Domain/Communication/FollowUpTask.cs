using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.Communication;

public class FollowUpTask : BaseEntity
{
    public Guid RelatedEntityId { get; set; }

    public string TaskType { get; set; } = null!;
    public DateTime DueDate { get; set; }
    public bool Completed { get; set; }
}

