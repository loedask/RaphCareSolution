using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Queries.GetConsultDisplay;

public sealed class GetConsultDisplayHandler(IAdminClinicConsultQueryService consultQueryService)
    : IRequestHandler<GetConsultDisplayQuery, ConsultDisplayBoardDto?>
{
    public Task<ConsultDisplayBoardDto?> Handle(
        GetConsultDisplayQuery request,
        CancellationToken cancellationToken) =>
        consultQueryService.GetDisplayBoardAsync(request.Token, cancellationToken);
}
