using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.CallAdminClinicCasualtyTicket;

public sealed class CallAdminClinicCasualtyTicketCommand : IRequest<AdminClinicCasualtyTicketDto?>
{
    public Guid ClinicId { get; set; }
    public Guid TicketId { get; set; }
}
