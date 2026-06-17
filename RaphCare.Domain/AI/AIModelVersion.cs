using RaphCare.Domain.Common;

namespace RaphCare.Domain.AI;

public class AIModelVersion : BaseEntity
{
    public string ModelName { get; set; } = null!;
    public string Version { get; set; } = null!;
    public DateTime DeployedAt { get; set; }
    public bool IsActive { get; set; }
}

