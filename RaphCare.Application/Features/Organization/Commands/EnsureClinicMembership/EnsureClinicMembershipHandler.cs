using MediatR;
using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Organization.Commands.EnsureClinicMembership;

public sealed class EnsureClinicMembershipHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IRepository<Clinic> clinicRepository) : IRequestHandler<EnsureClinicMembershipCommand, bool>
{
    public async Task<bool> Handle(EnsureClinicMembershipCommand request, CancellationToken cancellationToken)
    {
        if (currentUserService.CurrentUserId is not { } userId)
            return false;

        if (await clinicStaffMembershipService.HasMembershipAsync(userId, request.ClinicId, cancellationToken).ConfigureAwait(false))
            return true;

        var clinicPage = await clinicRepository.SearchAsync(
            queryShaper: q => q.Where(c => c.Id == request.ClinicId),
            pageNumber: 1,
            pageSize: 1,
            cancellationToken: cancellationToken).ConfigureAwait(false);
        var clinic = clinicPage.Items.FirstOrDefault();
        if (clinic is null)
            return false;

        var staffCount = await clinicStaffMembershipService
            .GetActiveStaffCountAsync(request.ClinicId, cancellationToken)
            .ConfigureAwait(false);

        var canClaim = clinic.RegisteredByApplicationUserId == userId || staffCount == 0;
        if (!canClaim)
            return false;

        await clinicStaffMembershipService.EnsureMembershipAsync(userId, request.ClinicId, cancellationToken).ConfigureAwait(false);
        return true;
    }
}
