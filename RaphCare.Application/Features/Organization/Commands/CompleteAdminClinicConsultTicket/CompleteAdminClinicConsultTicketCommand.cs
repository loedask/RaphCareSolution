using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.CompleteAdminClinicConsultTicket;

public sealed class CompleteAdminClinicConsultTicketCommand : IRequest<AdminClinicConsultTicketDto?>
{
    public Guid ClinicId { get; set; }
    public Guid TicketId { get; set; }
    public bool Cancel { get; set; }
}
