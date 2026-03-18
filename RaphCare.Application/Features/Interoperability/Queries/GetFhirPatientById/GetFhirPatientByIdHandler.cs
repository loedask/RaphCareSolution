using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Interoperability.DTOs;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.Interoperability.Queries.GetFhirPatientById;

/// <summary>
/// Phase 1: exports a patient as a minimal FHIR-shaped DTO.
/// </summary>
public class GetFhirPatientByIdHandler(
    IRepository<Patient> repository,
    IPatientFhirMapper mapper) : IRequestHandler<GetFhirPatientByIdQuery, FhirPatientDto>
{
    private readonly IRepository<Patient> _repository = repository;
    private readonly IPatientFhirMapper _mapper = mapper;

    public async Task<FhirPatientDto> Handle(GetFhirPatientByIdQuery request, CancellationToken cancellationToken)
    {
        var patient = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (patient is null)
            throw new NotFoundException(nameof(Patient), request.Id);

        return await _mapper.MapToDtoAsync(patient, cancellationToken).ConfigureAwait(false);
    }
}

