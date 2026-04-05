namespace RaphCare.Application.Features.Interoperability.DTOs;

/// <summary>Minimal FHIR Observation.component (phase 1 export).</summary>
public sealed class FhirObservationComponentDto
{
    public FhirCodeableConceptDto Code { get; set; } = new();
    public FhirQuantityDto? ValueQuantity { get; set; }
}
