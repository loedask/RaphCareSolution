using MediatR;
using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinicById;

public sealed class GetAdminClinicByIdHandler(
    IRepository<Clinic> clinicRepository,
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IUserRoleAssignmentService roleAssignmentService)
    : IRequestHandler<GetAdminClinicByIdQuery, ClinicDetailDto?>
{
    public async Task<ClinicDetailDto?> Handle(GetAdminClinicByIdQuery request, CancellationToken cancellationToken)
    {
        if (currentUserService.CurrentUserId is not { } userId)
            return null;

        if (!await clinicStaffMembershipService.HasMembershipAsync(userId, request.ClinicId, cancellationToken).ConfigureAwait(false))
            return null;

        var clinic = await clinicRepository.SearchAsync(
            queryShaper: q => q
                .Include(c => c.Facilities)
                .Where(c => c.Id == request.ClinicId),
            pageNumber: 1,
            pageSize: 1,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        var entity = clinic.Items.Count > 0 ? clinic.Items[0] : null;
        if (entity is null)
            return null;

        return await AdminClinicDetailMapper.MapAsync(
            entity,
            currentUserService,
            clinicStaffMembershipService,
            roleAssignmentService,
            cancellationToken).ConfigureAwait(false);
    }
}
