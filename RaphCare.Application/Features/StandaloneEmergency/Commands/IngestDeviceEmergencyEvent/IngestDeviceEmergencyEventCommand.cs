using System.Text.Json.Serialization;
using MediatR;
using RaphCare.Application.Features.StandaloneEmergency.DTOs;

namespace RaphCare.Application.Features.StandaloneEmergency.Commands.IngestDeviceEmergencyEvent;

/// <summary>Payload from cellular / OEM webhook (JSON body).</summary>
public sealed class IngestDeviceEmergencyEventCommand : IRequest<IngestDeviceEmergencyEventResult>
{
    [JsonPropertyName("serialNumber")]
    public string SerialNumber { get; set; } = string.Empty;

    [JsonPropertyName("eventType")]
    public string EventType { get; set; } = string.Empty;

    [JsonPropertyName("occurredAtUtc")]
    public DateTime OccurredAtUtc { get; set; }

    [JsonPropertyName("externalEventId")]
    public string? ExternalEventId { get; set; }

    [JsonPropertyName("latitude")]
    public double? Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double? Longitude { get; set; }

    [JsonPropertyName("horizontalAccuracyMeters")]
    public double? HorizontalAccuracyMeters { get; set; }
}
