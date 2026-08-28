using MediatR;
using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Identity;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Organization.Commands.RegisterClinic;

public sealed class RegisterClinicHandler(
    IRepository<Clinic> clinicRepository,
    IUnitOfWork unitOfWork,
    IUniqueConstraintViolationDetector uniqueConstraintDetector,
    ICurrentUserService currentUserService,
    IUserRoleAssignmentService roleAssignmentService,
    IClinicStaffMembershipService clinicStaffMembershipService) : IRequestHandler<RegisterClinicCommand, RegisterClinicResultDto>
{
    public async Task<RegisterClinicResultDto> Handle(RegisterClinicCommand request, CancellationToken cancellationToken)
    {
        var clinic = new Clinic
        {
            Name = request.Name.Trim(),
            RegistrationNumber = request.RegistrationNumber.Trim(),
            ReferenceCode = await AllocateReferenceCodeAsync(cancellationToken).ConfigureAwait(false),
            Country = request.Country.Trim(),
            TimeZone = request.TimeZone.Trim(),
            IsActive = true
        };

        Facility? facility = null;
        if (!string.IsNullOrWhiteSpace(request.FacilityName))
        {
            facility = new Facility
            {
                Name = request.FacilityName.Trim(),
                Address = request.FacilityAddress?.Trim() ?? string.Empty,
                City = request.FacilityCity?.Trim() ?? string.Empty,
                Country = request.Country.Trim(),
                IsVirtual = request.IsVirtualFacility
            };
            clinic.Facilities.Add(facility);
        }

        if (currentUserService.CurrentUserId is { } registeringUserId)
            clinic.RegisteredByApplicationUserId = registeringUserId;

        try
        {
            await clinicRepository.AddAsync(clinic, cancellationToken).ConfigureAwait(false);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateException ex) when (uniqueConstraintDetector.IsUniqueConstraintViolation(ex))
        {
            throw new InvalidOperationException(
                "Could not allocate a unique hospital reference. Try again.",
                ex);
        }

        if (currentUserService.CurrentUserId is { } userId)
        {
            await roleAssignmentService.AssignRoleIfMissingAsync(userId, RaphCareRoles.Administrator, cancellationToken).ConfigureAwait(false);
            await clinicStaffMembershipService.EnsureMembershipAsync(userId, clinic.Id, cancellationToken).ConfigureAwait(false);
        }

        return new RegisterClinicResultDto
        {
            ClinicId = clinic.Id,
            Name = clinic.Name,
            ReferenceCode = clinic.ReferenceCode,
            PrimaryFacilityId = facility?.Id,
            PrimaryFacilityName = facility?.Name
        };
    }

    private async Task<string> AllocateReferenceCodeAsync(CancellationToken cancellationToken)
    {
        const int maxAttempts = 8;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            var code = ClinicReferenceCode.Generate();
            var existing = await clinicRepository.SearchAsync(
                queryShaper: q => q.Where(c => c.ReferenceCode == code),
                pageNumber: 1,
                pageSize: 1,
                applyDefaultIdOrdering: false,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            if (existing.TotalCount == 0)
                return code;
        }

        throw new InvalidOperationException("Could not allocate a unique hospital reference. Try again.");
    }
}
