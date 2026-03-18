using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Interoperability.DTOs;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Interoperability.Queries.GetFhirOrganizations;

/// <summary>
/// Phase 1: minimal searchset export for Organization (Clinic).
/// </summary>
public class GetFhirOrganizationsHandler(
    IRepository<Clinic> repository,
    IOrganizationFhirMapper mapper) : IRequestHandler<GetFhirOrganizationsQuery, FhirBundleDto>
{
    private readonly IRepository<Clinic> _repository = repository;
    private readonly IOrganizationFhirMapper _mapper = mapper;

    public async Task<FhirBundleDto> Handle(GetFhirOrganizationsQuery request, CancellationToken cancellationToken)
    {
        var clinics = await _repository.ListAsync(cancellationToken).ConfigureAwait(false);

        var filtered = clinics.AsEnumerable();

        if (request.Active is bool active)
            filtered = filtered.Where(c => c.IsActive == active);

        var entries = new List<FhirBundleEntryDto>();
        foreach (var clinic in filtered)
        {
            var dto = await _mapper.MapToDtoAsync(clinic, cancellationToken).ConfigureAwait(false);
            entries.Add(new FhirBundleEntryDto
            {
                FullUrl = $"urn:raphcare:fhir:Organization/{clinic.Id}",
                Resource = dto
            });
        }

        return new FhirBundleDto
        {
            Total = entries.Count,
            Entry = entries
        };
    }
}

