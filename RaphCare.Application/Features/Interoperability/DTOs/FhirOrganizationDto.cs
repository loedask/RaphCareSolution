using System.Text.Json.Serialization;

namespace RaphCare.Application.Features.Interoperability.DTOs;

/// <summary>
/// FHIR (Fast Healthcare Interoperability Resources)-shaped Organization (phase 1 export, not a full FHIR implementation).
/// </summary>
public class FhirOrganizationDto
{
    [JsonPropertyName("resourceType")]
    public string ResourceType { get; set; } = "Organization";

    public string Id { get; set; } = string.Empty;

    public string? Name { get; set; }

    public List<FhirIdentifierDto> Identifier { get; set; } = new();

    public bool? Active { get; set; }
}

