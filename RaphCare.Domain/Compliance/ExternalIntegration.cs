using RaphCare.Domain.Common;

namespace RaphCare.Domain.Compliance;

/// <summary>
/// External integration (e.g. lab, pharmacy) for compliance tracking.
/// </summary>
public class ExternalIntegration : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
