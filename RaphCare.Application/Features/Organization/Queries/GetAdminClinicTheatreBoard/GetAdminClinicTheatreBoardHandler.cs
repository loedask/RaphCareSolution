using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinicTheatreBoard;

public sealed class GetAdminClinicTheatreBoardHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IAdminClinicTheatreQueryService theatreQueryService,
    IDateTimeProvider clock,
    IRepository<Clinic> clinicRepository)
    : IRequestHandler<GetAdminClinicTheatreBoardQuery, AdminClinicTheatreBoardDto?>
{
    public async Task<AdminClinicTheatreBoardDto?> Handle(
        GetAdminClinicTheatreBoardQuery request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.HasClinicAccessAsync(
                currentUserService, clinicStaffMembershipService, request.ClinicId, cancellationToken)
            .ConfigureAwait(false))
            return null;

        await AdminClinicCommercialPlanGuard.EnsureFeatureAsync(
                clinicRepository,
                request.ClinicId,
                ClinicCommercialPlanFeatures.HasTheatre,
                "Theatre",
                cancellationToken)
            .ConfigureAwait(false);

        var day = request.DayUtc ?? clock.UtcNow;
        return await theatreQueryService
            .GetBoardAsync(request.ClinicId, day, cancellationToken)
            .ConfigureAwait(false);
    }
}
