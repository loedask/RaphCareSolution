using MediatR;
using RaphCare.Application.Features.Interoperability.DTOs;

namespace RaphCare.Application.Features.Interoperability.Queries.GetFhirOrganizations;

/// <summary>
/// Phase 1: minimal searchset export for Organization (Clinic).
/// </summary>
public class GetFhirOrganizationsQuery : IRequest<FhirBundleDto>
{
    public bool? Active { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

