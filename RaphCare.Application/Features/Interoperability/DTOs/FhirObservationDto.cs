using System.Text.Json.Serialization;

namespace RaphCare.Application.Features.Interoperability.DTOs;

/// <summary>FHIR R4-shaped Observation for wearable vitals (phase 1 export).</summary>
public sealed class FhirObservationDto
{
    [JsonPropertyName("resourceType")]
    public string ResourceType { get; set; } = "Observation";

    public string Id { get; set; } = string.Empty;

    /// <summary>registered | preliminary | final | amended — we use <c>final</c> for stored readings.</summary>
    public string Status { get; set; } = "final";

    public List<FhirCodeableConceptDto>? Category { get; set; }

    public FhirCodeableConceptDto Code { get; set; } = new();

    public FhirReferenceDto Subject { get; set; } = new();

    [JsonPropertyName("effectiveDateTime")]
    public DateTimeOffset EffectiveDateTime { get; set; }

    public DateTimeOffset? Issued { get; set; }

    public FhirQuantityDto? ValueQuantity { get; set; }

    public List<FhirObservationComponentDto>? Component { get; set; }
}
