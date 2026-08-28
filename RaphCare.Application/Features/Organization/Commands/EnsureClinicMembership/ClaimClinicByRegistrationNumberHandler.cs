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

        var raw = request.RegistrationNumber.Trim();
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        var clinic = await FindClinicAsync(raw, cancellationToken).ConfigureAwait(false);
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

    private async Task<Clinic?> FindClinicAsync(string raw, CancellationToken cancellationToken)
    {
        if (ClinicReferenceCode.TryNormalize(raw, out var referenceCode))
        {
            var byCode = await clinicRepository.SearchAsync(
                queryShaper: q => q.Where(c => c.ReferenceCode == referenceCode),
                pageNumber: 1,
                pageSize: 1,
                applyDefaultIdOrdering: false,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            if (byCode.Items.Count == 1)
                return byCode.Items[0];
        }

        var byRegistration = await clinicRepository.SearchAsync(
            queryShaper: q => q.Where(c => c.RegistrationNumber == raw),
            pageNumber: 1,
            pageSize: 2,
            applyDefaultIdOrdering: false,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        return byRegistration.Items.Count == 1 ? byRegistration.Items[0] : null;
    }
}
