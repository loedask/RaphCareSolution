using System.Text.Json.Serialization;

namespace RaphCare.Application.Features.Interoperability.DTOs;

/// <summary>Minimal FHIR Quantity (phase 1 export).</summary>
public sealed class FhirQuantityDto
{
    [JsonPropertyName("value")]
    public decimal Value { get; set; }

    public string? Unit { get; set; }

    [JsonPropertyName("system")]
    public string System { get; set; } = "http://unitsofmeasure.org";

    public string? Code { get; set; }
}
