namespace RaphCare.Application.Features.Interoperability.DTOs;

/// <summary>
/// Minimal FHIR-shaped Period.
/// </summary>
public class FhirPeriodDto
{
    public DateTime? Start { get; set; }
    public DateTime? End { get; set; }
}

