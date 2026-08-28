using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Organization.Commands.SetAdminClinicProviderActive;

public sealed class SetAdminClinicProviderActiveHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IUserRoleAssignmentService roleAssignmentService,
    IRepository<Provider> providerRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<SetAdminClinicProviderActiveCommand, bool>
{
    public async Task<bool> Handle(SetAdminClinicProviderActiveCommand request, CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.IsClinicAdministratorAsync(
                currentUserService,
                clinicStaffMembershipService,
                roleAssignmentService,
                request.ClinicId,
                cancellationToken)
            .ConfigureAwait(false))
            throw new ForbiddenAccessException("Only hospital administrators can change provider status.");

        var provider = await providerRepository.GetByIdAsync(request.ProviderId, cancellationToken).ConfigureAwait(false);
        if (provider is null || provider.IsDeleted || provider.ClinicId != request.ClinicId)
            return false;

        if (provider.IsActive == request.IsActive)
            return true;

        provider.IsActive = request.IsActive;
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
