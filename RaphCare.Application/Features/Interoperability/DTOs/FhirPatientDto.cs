using System.Text.Json.Serialization;

namespace RaphCare.Application.Features.Interoperability.DTOs;

/// <summary>
/// FHIR-shaped Patient (phase 1 export, not a full FHIR implementation).
/// </summary>
public class FhirPatientDto
{
    [JsonPropertyName("resourceType")]
    public string ResourceType { get; set; } = "Patient";

    public string Id { get; set; } = string.Empty;

    public List<FhirIdentifierDto> Identifier { get; set; } = new();

    public List<FhirHumanNameDto> Name { get; set; } = new();

    public string? Gender { get; set; }

    [JsonPropertyName("birthDate")]
    public string? BirthDate { get; set; }

    public List<FhirContactPointDto> Telecom { get; set; } = new();
}

