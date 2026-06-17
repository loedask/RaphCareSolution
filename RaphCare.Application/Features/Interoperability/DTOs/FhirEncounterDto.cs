using System.Text.Json.Serialization;
using RaphCare.Application.Features.Interoperability.DTOs;

namespace RaphCare.Application.Features.Interoperability.DTOs;

/// <summary>
/// FHIR (Fast Healthcare Interoperability Resources)-shaped Encounter (phase 1 export, not a full FHIR implementation).
/// </summary>
public class FhirEncounterDto
{
    [JsonPropertyName("resourceType")]
    public string ResourceType { get; set; } = "Encounter";

    public string Id { get; set; } = string.Empty;

    public string? Status { get; set; }

    public FhirReferenceDto Subject { get; set; } = new();
    public FhirReferenceDto ServiceProvider { get; set; } = new();

    public FhirPeriodDto? Period { get; set; }

    public FhirReferenceDto? Appointment { get; set; }
}

