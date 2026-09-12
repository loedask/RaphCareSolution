using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinicConsultBoard;

public sealed class GetAdminClinicConsultBoardHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IAdminClinicConsultQueryService consultQueryService,
    IRepository<Clinic> clinicRepository)
    : IRequestHandler<GetAdminClinicConsultBoardQuery, AdminClinicConsultBoardDto?>
{
    public async Task<AdminClinicConsultBoardDto?> Handle(
        GetAdminClinicConsultBoardQuery request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.HasClinicAccessAsync(
                currentUserService, clinicStaffMembershipService, request.ClinicId, cancellationToken)
            .ConfigureAwait(false))
            return null;

        await AdminClinicCommercialPlanGuard.EnsureFeatureAsync(
                clinicRepository,
                request.ClinicId,
                ClinicCommercialPlanFeatures.HasConsultWaiting,
                "Consult waiting",
                cancellationToken)
            .ConfigureAwait(false);

        return await consultQueryService
            .GetBoardAsync(request.ClinicId, cancellationToken)
            .ConfigureAwait(false);
    }
}
