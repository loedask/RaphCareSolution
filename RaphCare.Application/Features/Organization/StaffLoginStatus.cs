using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Organization;

internal static class StaffLoginStatus
{
    public static async Task<bool> HasLoggedInAsync(
        Guid userId,
        Clinic? clinic,
        IProfessionalUserLookupService professionalUserLookupService,
        CancellationToken cancellationToken)
    {
        if (clinic?.RegisteredByApplicationUserId == userId)
            return true;

        return await professionalUserLookupService
            .HasSuccessfulLoginAsync(userId, cancellationToken)
            .ConfigureAwait(false);
    }
}
