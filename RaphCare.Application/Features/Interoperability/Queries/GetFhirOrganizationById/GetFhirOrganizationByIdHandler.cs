using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Interoperability.DTOs;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Interoperability.Queries.GetFhirOrganizationById;

/// <summary>
/// Phase 1: exports a Clinic as a minimal FHIR (Fast Healthcare Interoperability Resources)-shaped Organization DTO.
/// </summary>
public class GetFhirOrganizationByIdHandler(
    IRepository<Clinic> repository,
    IOrganizationFhirMapper mapper) : IRequestHandler<GetFhirOrganizationByIdQuery, FhirOrganizationDto>
{
    private readonly IRepository<Clinic> _repository = repository;
    private readonly IOrganizationFhirMapper _mapper = mapper;

    public async Task<FhirOrganizationDto> Handle(GetFhirOrganizationByIdQuery request, CancellationToken cancellationToken)
    {
        var clinic = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (clinic is null)
            throw new NotFoundException(nameof(Clinic), request.Id);

        return await _mapper.MapToDtoAsync(clinic, cancellationToken).ConfigureAwait(false);
    }
}

