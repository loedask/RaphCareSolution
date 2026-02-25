using RaphCare.Domain.Common;

namespace RaphCare.Domain.InfrastructureEntities;

/// <summary>
/// API key used for system-to-system authentication.
/// </summary>
public class APIKey : BaseEntity
{
    public string KeyName { get; set; } = string.Empty;
    public string KeyValue { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

