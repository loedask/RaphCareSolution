using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicWard;

public sealed class CreateAdminClinicWardHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IUserRoleAssignmentService roleAssignmentService,
    IRepository<Facility> facilityRepository,
    IRepository<Ward> wardRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateAdminClinicWardCommand, AdminClinicWardDto?>
{
    public async Task<AdminClinicWardDto?> Handle(CreateAdminClinicWardCommand request, CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.IsClinicAdministratorAsync(
                currentUserService, clinicStaffMembershipService, roleAssignmentService, request.ClinicId, cancellationToken)
            .ConfigureAwait(false))
            throw new ForbiddenAccessException("Only hospital administrators can create wards.");

        var facility = await facilityRepository.GetByIdAsync(request.FacilityId, cancellationToken).ConfigureAwait(false);
        if (facility is null || facility.ClinicId != request.ClinicId)
            return null;

        if (facility.IsVirtual)
            throw new BusinessRuleException("Cannot create wards on a virtual facility.");

        var ward = new Ward
        {
            ClinicId = request.ClinicId,
            FacilityId = facility.Id,
            Name = request.Name.Trim(),
            Code = string.IsNullOrWhiteSpace(request.Code) ? null : request.Code.Trim(),
            IsActive = true
        };

        await wardRepository.AddAsync(ward, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new AdminClinicWardDto
        {
            Id = ward.Id,
            FacilityId = ward.FacilityId,
            FacilityName = facility.Name,
            Name = ward.Name,
            Code = ward.Code,
            IsActive = ward.IsActive,
            Rooms = Array.Empty<AdminClinicRoomDto>()
        };
    }
}
