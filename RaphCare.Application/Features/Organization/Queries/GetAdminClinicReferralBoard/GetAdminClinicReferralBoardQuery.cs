using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinicReferralBoard;

public sealed class GetAdminClinicReferralBoardQuery : IRequest<AdminClinicReferralBoardDto?>
{
    public Guid ClinicId { get; set; }
}
