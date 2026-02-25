using RaphCare.Domain.Common;

namespace RaphCare.Domain.InfrastructureEntities;

/// <summary>
/// Feature flag toggle, scoped by environment.
/// </summary>
public class FeatureFlag : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public string Environment { get; set; } = string.Empty;
}

