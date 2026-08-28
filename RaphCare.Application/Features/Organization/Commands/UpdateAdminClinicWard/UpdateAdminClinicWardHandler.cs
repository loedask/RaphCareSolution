using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Organization.Commands.UpdateAdminClinicWard;

public sealed class UpdateAdminClinicWardHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IUserRoleAssignmentService roleAssignmentService,
    IRepository<Ward> wardRepository,
    IRepository<Facility> facilityRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateAdminClinicWardCommand, AdminClinicWardDto?>
{
    public async Task<AdminClinicWardDto?> Handle(UpdateAdminClinicWardCommand request, CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.IsClinicAdministratorAsync(
                currentUserService, clinicStaffMembershipService, roleAssignmentService, request.ClinicId, cancellationToken)
            .ConfigureAwait(false))
            throw new ForbiddenAccessException("Only hospital administrators can update wards.");

        var ward = await wardRepository.GetByIdAsync(request.WardId, cancellationToken).ConfigureAwait(false);
        if (ward is null || ward.ClinicId != request.ClinicId)
            return null;

        ward.Name = request.Name.Trim();
        ward.Code = string.IsNullOrWhiteSpace(request.Code) ? null : request.Code.Trim();
        ward.IsActive = request.IsActive;

        await wardRepository.UpdateAsync(ward, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var facility = await facilityRepository.GetByIdAsync(ward.FacilityId, cancellationToken).ConfigureAwait(false);

        return new AdminClinicWardDto
        {
            Id = ward.Id,
            FacilityId = ward.FacilityId,
            FacilityName = facility?.Name ?? "Facility",
            Name = ward.Name,
            Code = ward.Code,
            IsActive = ward.IsActive,
            Rooms = Array.Empty<AdminClinicRoomDto>()
        };
    }
}
