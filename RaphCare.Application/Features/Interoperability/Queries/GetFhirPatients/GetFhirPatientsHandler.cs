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
        var patients = await _repository.ListAsync(cancellationToken).ConfigureAwait(false);

        var filtered = patients.AsEnumerable();

        if (request.Id is Guid id)
            filtered = filtered.Where(p => p.Id == id);

        if (!string.IsNullOrWhiteSpace(request.NationalHealthId))
        {
            var nhid = request.NationalHealthId.Trim();
            filtered = filtered.Where(p =>
                string.Equals(p.NationalHealthId, nhid, StringComparison.OrdinalIgnoreCase));
        }

        var entries = new List<FhirBundleEntryDto>();
        foreach (var patient in filtered)
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
            Total = entries.Count,
            Entry = entries
        };
    }
}

