using RaphCare.Domain.Common;

namespace RaphCare.Domain.Compliance;

/// <summary>
/// Log of external integration calls for compliance and debugging.
/// </summary>
public class IntegrationLog : BaseEntity
{
    public Guid ExternalIntegrationId { get; set; }
    public string RequestPayload { get; set; } = string.Empty;
    public string ResponsePayload { get; set; } = string.Empty;
    public bool Successful { get; set; }
    public DateTime ExecutedAt { get; set; }

    public ExternalIntegration ExternalIntegration { get; set; } = null!;
}
