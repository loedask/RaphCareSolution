using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinicTheatreBoard;

public sealed class GetAdminClinicTheatreBoardHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IAdminClinicTheatreQueryService theatreQueryService,
    IDateTimeProvider clock)
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

        var day = request.DayUtc ?? clock.UtcNow;
        return await theatreQueryService
            .GetBoardAsync(request.ClinicId, day, cancellationToken)
            .ConfigureAwait(false);
    }
}
