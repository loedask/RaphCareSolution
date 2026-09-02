using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Identity;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Organization;

internal static class AdminClinicDetailMapper
{
    public static async Task<ClinicDetailDto> MapAsync(
        Clinic clinic,
        ICurrentUserService currentUserService,
        IClinicStaffMembershipService clinicStaffMembershipService,
        IUserRoleAssignmentService roleAssignmentService,
        CancellationToken cancellationToken)
    {
        var roles = await AdminClinicAuthorization.GetClinicStaffRolesAsync(
            currentUserService,
            clinicStaffMembershipService,
            roleAssignmentService,
            clinic.Id,
            cancellationToken).ConfigureAwait(false);

        return new ClinicDetailDto
        {
            Id = clinic.Id,
            Name = clinic.Name,
            RegistrationNumber = clinic.RegistrationNumber,
            ReferenceCode = clinic.ReferenceCode,
            Country = clinic.Country,
            TimeZone = clinic.TimeZone,
            IsActive = clinic.IsActive,
            CreatedAt = clinic.CreatedAt,
            RegisteredByApplicationUserId = clinic.RegisteredByApplicationUserId,
            CurrentUserIsAdministrator = RaphCareRoles.HasAdministratorRole(roles),
            CurrentUserCanDocumentVisits = RaphCareRoles.CanDocumentVisits(roles),
            CurrentUserCanRecordWardNotes = RaphCareRoles.CanRecordWardNotes(roles),
            CurrentUserCanDispense = RaphCareRoles.CanDispensePrescriptions(roles),
            CurrentUserCanCompleteLabs = RaphCareRoles.CanCompleteLabs(roles),
            Facilities = clinic.Facilities
                .OrderBy(f => f.Name)
                .Select(f => new FacilityListItemDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    Address = f.Address,
                    City = f.City,
                    Country = f.Country,
                    IsVirtual = f.IsVirtual
                })
                .ToList()
        };
    }
}
