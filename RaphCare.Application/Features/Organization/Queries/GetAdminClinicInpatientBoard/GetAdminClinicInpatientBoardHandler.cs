using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinicInpatientBoard;

public sealed class GetAdminClinicInpatientBoardHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IAdminClinicInpatientQueryService inpatientQueryService,
    IRepository<Clinic> clinicRepository)
    : IRequestHandler<GetAdminClinicInpatientBoardQuery, AdminClinicInpatientBoardDto?>
{
    public async Task<AdminClinicInpatientBoardDto?> Handle(
        GetAdminClinicInpatientBoardQuery request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.HasClinicAccessAsync(
                currentUserService, clinicStaffMembershipService, request.ClinicId, cancellationToken)
            .ConfigureAwait(false))
            return null;

        await AdminClinicCommercialPlanGuard.EnsureFeatureAsync(
                clinicRepository,
                request.ClinicId,
                ClinicCommercialPlanFeatures.HasInpatient,
                "Inpatient",
                cancellationToken)
            .ConfigureAwait(false);

        return await inpatientQueryService.GetBoardAsync(request.ClinicId, cancellationToken).ConfigureAwait(false);
    }
}
