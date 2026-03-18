namespace RaphCare.Application.Features.Interoperability.DTOs;

/// <summary>
/// Minimal FHIR-shaped HumanName.
/// </summary>
public class FhirHumanNameDto
{
    public string? Family { get; set; }
    public List<string> Given { get; set; } = new();
}

