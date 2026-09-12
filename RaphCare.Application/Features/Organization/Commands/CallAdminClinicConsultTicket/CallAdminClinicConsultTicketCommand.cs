using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.CallAdminClinicConsultTicket;

public sealed class CallAdminClinicConsultTicketCommand : IRequest<AdminClinicConsultTicketDto?>
{
    public Guid ClinicId { get; set; }
    public Guid TicketId { get; set; }
}
