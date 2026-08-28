using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Clinical;

namespace RaphCare.Application.Features.Organization;

internal static class AdminClinicVisitDocumentationHelper
{
    public static async Task<Visit?> GetWritableVisitAsync(
        ICurrentUserService currentUser,
        IClinicStaffMembershipService membershipService,
        IUserRoleAssignmentService roleAssignmentService,
        IRepository<Visit> visitRepository,
        Guid clinicId,
        Guid visitId,
        string forbiddenMessage,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.IsClinicAdministratorAsync(
                currentUser,
                membershipService,
                roleAssignmentService,
                clinicId,
                cancellationToken)
            .ConfigureAwait(false))
            throw new ForbiddenAccessException(forbiddenMessage);

        var visit = await visitRepository.GetByIdAsync(visitId, cancellationToken).ConfigureAwait(false);
        if (visit is null || visit.ClinicId != clinicId)
            return null;

        if (visit.Status is "Completed" or "Cancelled")
            throw new BusinessRuleException("Cannot update documentation on a closed visit.");

        return visit;
    }
}
