using RaphCare.Domain.Common;

namespace RaphCare.Domain.Compliance;

/// <summary>
/// Regulatory framework (e.g. POPIA, HIPAA, GDPR) configuration.
/// </summary>
public class RegulatoryFramework : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
