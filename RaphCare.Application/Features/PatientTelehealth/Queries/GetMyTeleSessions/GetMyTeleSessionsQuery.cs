using MediatR;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Features.PatientTelehealth.DTOs;

namespace RaphCare.Application.Features.PatientTelehealth.Queries.GetMyTeleSessions;

public class GetMyTeleSessionsQuery : IRequest<PagedResult<PatientTeleSessionListItemDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
