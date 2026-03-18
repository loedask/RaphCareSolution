using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Interoperability.DTOs;
using RaphCare.Domain.Clinical;

namespace RaphCare.Application.Features.Interoperability.Queries.GetFhirEncounters;

/// <summary>
/// Phase 1: minimal searchset export for Encounter (Visit).
/// </summary>
public class GetFhirEncountersHandler(
    IRepository<Visit> repository,
    IEncounterFhirMapper mapper) : IRequestHandler<GetFhirEncountersQuery, FhirBundleDto>
{
    private readonly IRepository<Visit> _repository = repository;
    private readonly IEncounterFhirMapper _mapper = mapper;

    public async Task<FhirBundleDto> Handle(GetFhirEncountersQuery request, CancellationToken cancellationToken)
    {
        var visits = await _repository.ListAsync(cancellationToken).ConfigureAwait(false);

        var filtered = visits.AsEnumerable();

        if (request.PatientId is Guid patientId)
            filtered = filtered.Where(v => v.PatientId == patientId);

        if (request.ClinicId is Guid clinicId)
            filtered = filtered.Where(v => v.ClinicId == clinicId);

        var entries = new List<FhirBundleEntryDto>();
        foreach (var visit in filtered)
        {
            var dto = await _mapper.MapToDtoAsync(visit, cancellationToken).ConfigureAwait(false);
            entries.Add(new FhirBundleEntryDto
            {
                FullUrl = $"urn:raphcare:fhir:Encounter/{visit.Id}",
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

