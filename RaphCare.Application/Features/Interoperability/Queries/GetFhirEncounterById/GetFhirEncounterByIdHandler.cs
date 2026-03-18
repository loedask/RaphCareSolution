using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Interoperability.DTOs;
using RaphCare.Domain.Clinical;

namespace RaphCare.Application.Features.Interoperability.Queries.GetFhirEncounterById;

/// <summary>
/// Phase 1: exports a Visit as a minimal FHIR-shaped Encounter DTO.
/// </summary>
public class GetFhirEncounterByIdHandler(
    IRepository<Visit> repository,
    IEncounterFhirMapper mapper) : IRequestHandler<GetFhirEncounterByIdQuery, FhirEncounterDto>
{
    private readonly IRepository<Visit> _repository = repository;
    private readonly IEncounterFhirMapper _mapper = mapper;

    public async Task<FhirEncounterDto> Handle(GetFhirEncounterByIdQuery request, CancellationToken cancellationToken)
    {
        var visit = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (visit is null)
            throw new NotFoundException(nameof(Visit), request.Id);

        return await _mapper.MapToDtoAsync(visit, cancellationToken).ConfigureAwait(false);
    }
}

