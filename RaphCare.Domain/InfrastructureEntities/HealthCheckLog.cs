using RaphCare.Domain.Common;

namespace RaphCare.Domain.InfrastructureEntities;

/// <summary>
/// Health check result for a downstream or internal service.
/// </summary>
public class HealthCheckLog : BaseEntity
{
    public string ServiceName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CheckedAt { get; set; }
}

