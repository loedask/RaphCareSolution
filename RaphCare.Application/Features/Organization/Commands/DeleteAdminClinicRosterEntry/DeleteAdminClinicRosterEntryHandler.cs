using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Organization.Commands.DeleteAdminClinicRosterEntry;

public sealed class DeleteAdminClinicRosterEntryHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IRepository<ClinicRosterEntry> rosterRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteAdminClinicRosterEntryCommand, bool>
{
    public async Task<bool> Handle(
        DeleteAdminClinicRosterEntryCommand request,
        CancellationToken cancellationToken)
    {
        await AdminClinicAuthorization.EnsureClinicStaffAsync(
                currentUserService,
                clinicStaffMembershipService,
                request.ClinicId,
                "Only hospital staff can update the roster.",
                cancellationToken)
            .ConfigureAwait(false);

        var entry = await rosterRepository.GetByIdAsync(request.EntryId, cancellationToken).ConfigureAwait(false);
        if (entry is null || entry.ClinicId != request.ClinicId)
            return false;

        await rosterRepository.DeleteAsync(entry, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
