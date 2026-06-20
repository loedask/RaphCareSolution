using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Organization.Commands.EnsureClinicMembership;

public sealed class ClaimClinicByRegistrationNumberHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IRepository<Clinic> clinicRepository) : IRequestHandler<ClaimClinicByRegistrationNumberCommand, Guid?>
{
    public async Task<Guid?> Handle(ClaimClinicByRegistrationNumberCommand request, CancellationToken cancellationToken)
    {
        if (currentUserService.CurrentUserId is not { } userId)
            return null;

        var registrationNumber = request.RegistrationNumber.Trim();
        if (string.IsNullOrWhiteSpace(registrationNumber))
            return null;

        var page = await clinicRepository.SearchAsync(
            queryShaper: q => q.Where(c => c.RegistrationNumber == registrationNumber),
            pageNumber: 1,
            pageSize: 1,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        var clinic = page.Items.FirstOrDefault();
        if (clinic is null)
            return null;

        if (await clinicStaffMembershipService.HasMembershipAsync(userId, clinic.Id, cancellationToken).ConfigureAwait(false))
            return clinic.Id;

        var staffCount = await clinicStaffMembershipService
            .GetActiveStaffCountAsync(clinic.Id, cancellationToken)
            .ConfigureAwait(false);

        if (clinic.RegisteredByApplicationUserId != userId && staffCount > 0)
            return null;

        await clinicStaffMembershipService.EnsureMembershipAsync(userId, clinic.Id, cancellationToken).ConfigureAwait(false);
        return clinic.Id;
    }
}
