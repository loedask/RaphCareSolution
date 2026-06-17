using RaphCare.Domain.Common;

namespace RaphCare.Domain.InfrastructureEntities;

/// <summary>
/// Message enqueued for background processing.
/// </summary>
public class QueueMessage : BaseEntity
{
    public string MessageType { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
    public bool Processed { get; set; }
    public DateTime EnqueuedAt { get; set; }
}

