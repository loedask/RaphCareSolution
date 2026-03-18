using MediatR;
using RaphCare.Application.Features.Interoperability.DTOs;

namespace RaphCare.Application.Features.Interoperability.Queries.GetFhirPatients;

/// <summary>
/// Phase 1: minimal searchset export for Patient.
/// </summary>
public class GetFhirPatientsQuery : IRequest<FhirBundleDto>
{
    public Guid? Id { get; set; }
    public string? NationalHealthId { get; set; }
}

