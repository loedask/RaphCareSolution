using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinicReferralBoard;

public sealed class GetAdminClinicReferralBoardHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IAdminClinicReferralQueryService referralQueryService)
    : IRequestHandler<GetAdminClinicReferralBoardQuery, AdminClinicReferralBoardDto?>
{
    public async Task<AdminClinicReferralBoardDto?> Handle(
        GetAdminClinicReferralBoardQuery request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.HasClinicAccessAsync(
                currentUserService, clinicStaffMembershipService, request.ClinicId, cancellationToken)
            .ConfigureAwait(false))
            return null;

        return await referralQueryService
            .GetBoardAsync(request.ClinicId, cancellationToken)
            .ConfigureAwait(false);
    }
}
