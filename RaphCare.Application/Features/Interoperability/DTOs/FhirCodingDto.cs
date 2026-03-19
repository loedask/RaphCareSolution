namespace RaphCare.Application.Features.Interoperability.DTOs;

/// <summary>
/// Minimal FHIR (Fast Healthcare Interoperability Resources)-shaped Coding.
/// </summary>
public class FhirCodingDto
{
    public string System { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Display { get; set; }
}

