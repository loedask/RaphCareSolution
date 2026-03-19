namespace RaphCare.Application.Features.Interoperability.DTOs;

/// <summary>
/// Minimal FHIR (Fast Healthcare Interoperability Resources)-shaped Period.
/// </summary>
public class FhirPeriodDto
{
    public DateTime? Start { get; set; }
    public DateTime? End { get; set; }
}

