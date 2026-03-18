namespace RaphCare.Application.Features.Interoperability.DTOs;

/// <summary>
/// Minimal FHIR-shaped CodeableConcept.
/// </summary>
public class FhirCodeableConceptDto
{
    public List<FhirCodingDto> Coding { get; set; } = new();
    public string? Text { get; set; }
}

