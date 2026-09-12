using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicConsultTicket;

public sealed class CreateAdminClinicConsultTicketCommand : IRequest<AdminClinicConsultTicketDto?>
{
    public Guid ClinicId { get; set; }
    public Guid AppointmentId { get; set; }
}
