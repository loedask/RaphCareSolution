using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Queries.GetCasualtyDisplay;

public sealed class GetCasualtyDisplayHandler(IAdminClinicCasualtyQueryService casualtyQueryService)
    : IRequestHandler<GetCasualtyDisplayQuery, CasualtyDisplayBoardDto?>
{
    public Task<CasualtyDisplayBoardDto?> Handle(
        GetCasualtyDisplayQuery request,
        CancellationToken cancellationToken) =>
        casualtyQueryService.GetDisplayBoardAsync(request.Token, cancellationToken);
}
