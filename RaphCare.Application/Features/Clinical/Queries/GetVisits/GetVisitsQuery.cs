using MediatR;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Features.Clinical.DTOs;

namespace RaphCare.Application.Features.Clinical.Queries.GetVisits;

public class GetVisitsQuery : IRequest<PagedResult<VisitDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

