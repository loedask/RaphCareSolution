using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinicCasualtyBoard;

public sealed class GetAdminClinicCasualtyBoardQuery : IRequest<AdminClinicCasualtyBoardDto?>
{
    public Guid ClinicId { get; set; }
}
