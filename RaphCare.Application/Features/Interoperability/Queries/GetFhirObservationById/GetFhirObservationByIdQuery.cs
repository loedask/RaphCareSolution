using MediatR;
using RaphCare.Application.Features.Interoperability.DTOs;

namespace RaphCare.Application.Features.Interoperability.Queries.GetFhirObservationById;

public sealed class GetFhirObservationByIdQuery : IRequest<FhirObservationDto>
{
    public Guid Id { get; set; }
}
