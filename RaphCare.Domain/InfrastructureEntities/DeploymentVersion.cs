using RaphCare.Domain.Common;

namespace RaphCare.Domain.InfrastructureEntities;

/// <summary>
/// Deployed application version information.
/// </summary>
public class DeploymentVersion : BaseEntity
{
    public string VersionNumber { get; set; } = string.Empty;
    public DateTime DeployedAt { get; set; }
    public string Environment { get; set; } = string.Empty;
}

