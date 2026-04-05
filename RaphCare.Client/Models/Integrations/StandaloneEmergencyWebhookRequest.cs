namespace RaphCare.Client.Models.Integrations;

/// <summary>JSON body for <c>POST api/integrations/standalone-emergency/events</c> (camelCase on the wire).</summary>
public sealed class StandaloneEmergencyWebhookRequest
{
    public string SerialNumber { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateTime OccurredAtUtc { get; set; }
    public string? ExternalEventId { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? HorizontalAccuracyMeters { get; set; }
}
