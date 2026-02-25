using RaphCare.Domain.Common;
using RaphCare.Domain.Common.Interfaces;

namespace RaphCare.Domain.AI;

public class AIModelVersion : BaseEntity
{
    public string ModelName { get; set; } = null!;
    public string Version { get; set; } = null!;
    public DateTime DeployedAt { get; set; }
    public bool IsActive { get; set; }
}

