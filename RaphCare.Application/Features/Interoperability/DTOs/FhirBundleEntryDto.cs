namespace RaphCare.Application.Features.Interoperability.DTOs;

/// <summary>
/// Minimal FHIR-shaped Bundle.entry element.
/// </summary>
public class FhirBundleEntryDto
{
    public string FullUrl { get; set; } = string.Empty;

    // Keep `object` so we can embed different minimal FHIR DTOs.
    public object Resource { get; set; } = new();
}

