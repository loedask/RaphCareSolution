namespace RaphCare.Application.Features.Interoperability.DTOs;

/// <summary>
/// Minimal FHIR (Fast Healthcare Interoperability Resources)-shaped CodeableConcept.
/// </summary>
public class FhirCodeableConceptDto
{
    public List<FhirCodingDto> Coding { get; set; } = new();
    public string? Text { get; set; }
}

