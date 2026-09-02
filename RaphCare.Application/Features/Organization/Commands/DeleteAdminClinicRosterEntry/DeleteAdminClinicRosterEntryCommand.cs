using MediatR;

namespace RaphCare.Application.Features.Organization.Commands.DeleteAdminClinicRosterEntry;

public sealed class DeleteAdminClinicRosterEntryCommand : IRequest<bool>
{
    public Guid ClinicId { get; set; }
    public Guid EntryId { get; set; }
}
