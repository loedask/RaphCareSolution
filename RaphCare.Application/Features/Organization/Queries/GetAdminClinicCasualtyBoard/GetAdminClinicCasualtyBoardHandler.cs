using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinicCasualtyBoard;

public sealed class GetAdminClinicCasualtyBoardHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IAdminClinicCasualtyQueryService casualtyQueryService,
    IRepository<Clinic> clinicRepository)
    : IRequestHandler<GetAdminClinicCasualtyBoardQuery, AdminClinicCasualtyBoardDto?>
{
    public async Task<AdminClinicCasualtyBoardDto?> Handle(
        GetAdminClinicCasualtyBoardQuery request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.HasClinicAccessAsync(
                currentUserService, clinicStaffMembershipService, request.ClinicId, cancellationToken)
            .ConfigureAwait(false))
            return null;

        await AdminClinicCommercialPlanGuard.EnsureFeatureAsync(
                clinicRepository,
                request.ClinicId,
                ClinicCommercialPlanFeatures.HasCasualty,
                "Casualty",
                cancellationToken)
            .ConfigureAwait(false);

        return await casualtyQueryService
            .GetBoardAsync(request.ClinicId, cancellationToken)
            .ConfigureAwait(false);
    }
}
