namespace RaphCare.Application.Features.Interoperability.DTOs;

/// <summary>
/// Minimal FHIR-shaped ContactPoint.
/// </summary>
public class FhirContactPointDto
{
    public string System { get; set; } = string.Empty; // phone, email
    public string Value { get; set; } = string.Empty;
    public string? Use { get; set; }
}

