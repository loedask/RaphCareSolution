using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Interoperability.DTOs;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.Interoperability.Queries.GetFhirPatients;

/// <summary>
/// Phase 1: minimal searchset export for Patient.
/// </summary>
public class GetFhirPatientsHandler(
    IRepository<Patient> repository,
    IPatientFhirMapper mapper) : IRequestHandler<GetFhirPatientsQuery, FhirBundleDto>
{
    private readonly IRepository<Patient> _repository = repository;
    private readonly IPatientFhirMapper _mapper = mapper;

    public async Task<FhirBundleDto> Handle(GetFhirPatientsQuery request, CancellationToken cancellationToken)
    {
        string? nhidLower = null;
        if (!string.IsNullOrWhiteSpace(request.NationalHealthId))
            nhidLower = request.NationalHealthId.Trim().ToLowerInvariant();

        var pagedPatients = await _repository.SearchAsync(
            queryShaper: q =>
            {
                if (request.Id is Guid id)
                    q = q.Where(p => p.Id == id);

                if (nhidLower is not null)
                    q = q.Where(p => p.NationalHealthId != null && p.NationalHealthId.ToLowerInvariant() == nhidLower);

                return q;
            },
            pageNumber: request.PageNumber,
            pageSize: request.PageSize,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        var entries = new List<FhirBundleEntryDto>();
        foreach (var patient in pagedPatients.Items)
        {
            var dto = await _mapper.MapToDtoAsync(patient, cancellationToken).ConfigureAwait(false);
            entries.Add(new FhirBundleEntryDto
            {
                FullUrl = $"urn:raphcare:fhir:Patient/{patient.Id}",
                Resource = dto
            });
        }

        return new FhirBundleDto
        {
            Total = pagedPatients.TotalCount,
            Entry = entries
        };
    }
}

