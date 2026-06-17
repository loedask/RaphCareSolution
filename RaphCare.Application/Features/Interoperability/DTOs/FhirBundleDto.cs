using System.Text.Json.Serialization;

namespace RaphCare.Application.Features.Interoperability.DTOs;

/// <summary>
/// Minimal FHIR (Fast Healthcare Interoperability Resources)-shaped Bundle (phase 1 searchset, not full FHIR).
/// </summary>
public class FhirBundleDto
{
    [JsonPropertyName("resourceType")]
    public string ResourceType { get; set; } = "Bundle";

    [JsonPropertyName("type")]
    public string Type { get; set; } = "searchset";

    public int Total { get; set; }

    public List<FhirBundleEntryDto> Entry { get; set; } = new();
}

