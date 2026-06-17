namespace RaphCare.Application.Features.Interoperability.DTOs;

/// <summary>
/// Minimal FHIR (Fast Healthcare Interoperability Resources)-shaped Identifier.
/// </summary>
public class FhirIdentifierDto
{
    public string System { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

