using MediatR;
using RaphCare.Application.Features.Interoperability.DTOs;

namespace RaphCare.Application.Features.Interoperability.Queries.GetFhirEncounters;

/// <summary>
/// Phase 1: minimal searchset export for Encounter (FHIR Encounter) based on clinic-scoped Visit.
/// </summary>
public class GetFhirEncountersQuery : IRequest<FhirBundleDto>
{
    public Guid? PatientId { get; set; }
    public Guid? ClinicId { get; set; }
}

