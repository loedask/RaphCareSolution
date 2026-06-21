using MediatR;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinicAppointments;

public sealed class GetAdminClinicAppointmentsHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IAdminClinicAppointmentQueryService adminClinicAppointmentQueryService)
    : IRequestHandler<GetAdminClinicAppointmentsQuery, PagedResult<AdminClinicAppointmentListItemDto>?>
{
    public async Task<PagedResult<AdminClinicAppointmentListItemDto>?> Handle(
        GetAdminClinicAppointmentsQuery request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.HasClinicAccessAsync(
                currentUserService, clinicStaffMembershipService, request.ClinicId, cancellationToken)
            .ConfigureAwait(false))
            return null;

        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 20 : Math.Min(request.PageSize, 100);

        return await adminClinicAppointmentQueryService
            .GetAppointmentsAsync(
                request.ClinicId,
                pageNumber,
                pageSize,
                request.FromUtc,
                request.ToUtc,
                request.Status,
                cancellationToken)
            .ConfigureAwait(false);
    }
}
