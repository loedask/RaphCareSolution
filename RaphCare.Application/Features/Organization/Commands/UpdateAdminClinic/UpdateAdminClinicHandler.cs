using MediatR;
using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Organization.Commands.UpdateAdminClinic;

public sealed class UpdateAdminClinicHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IUserRoleAssignmentService roleAssignmentService,
    IRepository<Clinic> clinicRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateAdminClinicCommand, ClinicDetailDto?>
{
    public async Task<ClinicDetailDto?> Handle(UpdateAdminClinicCommand request, CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.HasClinicAccessAsync(
                currentUserService, clinicStaffMembershipService, request.ClinicId, cancellationToken)
            .ConfigureAwait(false))
            return null;

        var clinic = await clinicRepository.GetByIdAsync(request.ClinicId, cancellationToken).ConfigureAwait(false);
        if (clinic is null || clinic.IsDeleted)
            return null;

        clinic.Name = request.Name.Trim();
        clinic.Country = request.Country.Trim();
        clinic.TimeZone = request.TimeZone.Trim();

        await clinicRepository.UpdateAsync(clinic, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var page = await clinicRepository.SearchAsync(
            queryShaper: q => q.Include(c => c.Facilities).Where(c => c.Id == clinic.Id),
            pageNumber: 1,
            pageSize: 1,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        var updated = page.Items.FirstOrDefault();
        if (updated is null)
            return null;

        return await AdminClinicDetailMapper.MapAsync(
            updated,
            currentUserService,
            clinicStaffMembershipService,
            roleAssignmentService,
            cancellationToken).ConfigureAwait(false);
    }
}
