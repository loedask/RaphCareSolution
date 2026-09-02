using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.CompleteAdminClinicCasualtyTicket;

public sealed class CompleteAdminClinicCasualtyTicketCommand : IRequest<AdminClinicCasualtyTicketDto?>
{
    public Guid ClinicId { get; set; }
    public Guid TicketId { get; set; }
    public bool Cancel { get; set; }
}
