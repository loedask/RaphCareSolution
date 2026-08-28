using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicProvider;

public sealed class CreateAdminClinicProviderHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IUserRoleAssignmentService roleAssignmentService,
    IProfessionalUserLookupService professionalUserLookupService,
    IRepository<Provider> providerRepository,
    IAdminClinicProviderQueryService adminClinicProviderQueryService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateAdminClinicProviderCommand, AdminClinicProviderListItemDto?>
{
    public async Task<AdminClinicProviderListItemDto?> Handle(
        CreateAdminClinicProviderCommand request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.IsClinicAdministratorAsync(
                currentUserService,
                clinicStaffMembershipService,
                roleAssignmentService,
                request.ClinicId,
                cancellationToken)
            .ConfigureAwait(false))
            throw new ForbiddenAccessException("Only hospital administrators can add providers.");

        if (!await clinicStaffMembershipService
                .HasMembershipAsync(request.UserId, request.ClinicId, cancellationToken)
                .ConfigureAwait(false))
            throw new BusinessRuleException("That user is not linked to this hospital as staff.");

        var existing = await providerRepository.SearchAsync(
            q => q.Where(p => p.ClinicId == request.ClinicId && p.ApplicationUserId == request.UserId && !p.IsDeleted),
            pageNumber: 1,
            pageSize: 1,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        if (existing.Items.Count > 0)
        {
            return (await adminClinicProviderQueryService
                .GetProvidersAsync(request.ClinicId, cancellationToken)
                .ConfigureAwait(false))
                .FirstOrDefault(p => p.ApplicationUserId == request.UserId);
        }

        var provider = new Provider
        {
            ClinicId = request.ClinicId,
            ApplicationUserId = request.UserId,
            LicenseNumber = string.IsNullOrWhiteSpace(request.LicenseNumber) ? string.Empty : request.LicenseNumber.Trim(),
            IsActive = true,
            JoinedAt = DateTime.UtcNow
        };

        await providerRepository.AddAsync(provider, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var users = await professionalUserLookupService
            .GetUsersByIdsAsync([request.UserId], cancellationToken)
            .ConfigureAwait(false);
        var user = users.Count > 0 ? users[0] : null;

        return new AdminClinicProviderListItemDto
        {
            ProviderId = provider.Id,
            ApplicationUserId = provider.ApplicationUserId,
            DisplayName = user?.DisplayName ?? user?.Email ?? "Provider",
            Email = user?.Email ?? string.Empty,
            LicenseNumber = provider.LicenseNumber,
            IsActive = provider.IsActive,
            ScheduleSlotCount = 0
        };
    }
}
