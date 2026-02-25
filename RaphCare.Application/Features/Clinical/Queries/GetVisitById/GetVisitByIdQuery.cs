using MediatR;
using RaphCare.Application.Features.Clinical.DTOs;

namespace RaphCare.Application.Features.Clinical.Queries.GetVisitById;

public class GetVisitByIdQuery : IRequest<VisitDto>
{
    public Guid Id { get; set; }
}

