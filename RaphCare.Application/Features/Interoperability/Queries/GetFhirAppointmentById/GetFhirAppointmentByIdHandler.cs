using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Interoperability.DTOs;
using RaphCare.Domain.Clinical;

namespace RaphCare.Application.Features.Interoperability.Queries.GetFhirAppointmentById;

/// <summary>
/// Phase 1: exports a single Appointment as a minimal FHIR-shaped DTO.
/// </summary>
public class GetFhirAppointmentByIdHandler(
    IRepository<Appointment> repository,
    IAppointmentFhirMapper mapper) : IRequestHandler<GetFhirAppointmentByIdQuery, FhirAppointmentDto>
{
    private readonly IRepository<Appointment> _repository = repository;
    private readonly IAppointmentFhirMapper _mapper = mapper;

    public async Task<FhirAppointmentDto> Handle(GetFhirAppointmentByIdQuery request, CancellationToken cancellationToken)
    {
        var appointment = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (appointment is null)
            throw new NotFoundException(nameof(Appointment), request.Id);

        return await _mapper.MapToDtoAsync(appointment, cancellationToken).ConfigureAwait(false);
    }
}

