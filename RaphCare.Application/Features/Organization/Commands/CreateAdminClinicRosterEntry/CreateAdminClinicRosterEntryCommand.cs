using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicRosterEntry;

public sealed class CreateAdminClinicRosterEntryCommand : IRequest<AdminClinicRosterEntryDto?>
{
    public Guid ClinicId { get; set; }
    public Guid ApplicationUserId { get; set; }
    public DateTime DutyDate { get; set; }
    public string ShiftLabel { get; set; } = ClinicRosterShift.Morning;
    public string? Note { get; set; }
}
