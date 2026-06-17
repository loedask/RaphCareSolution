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
        var pagedOrganizations = await _repository.SearchAsync(
            queryShaper: q =>
            {
                if (request.Active is bool active)
                    q = q.Where(c => c.IsActive == active);

                return q;
            },
            pageNumber: request.PageNumber,
            pageSize: request.PageSize,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        var entries = new List<FhirBundleEntryDto>();
        foreach (var clinic in pagedOrganizations.Items)
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
            Total = pagedOrganizations.TotalCount,
            Entry = entries
        };
    }
}

