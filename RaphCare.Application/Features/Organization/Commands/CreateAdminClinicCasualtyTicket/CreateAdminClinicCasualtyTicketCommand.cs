using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicCasualtyTicket;

public sealed class CreateAdminClinicCasualtyTicketCommand : IRequest<AdminClinicCasualtyTicketDto?>
{
    public Guid ClinicId { get; set; }
    public Guid? PatientId { get; set; }
    public string TriageLevel { get; set; } = "Green";
    public string? ChiefComplaint { get; set; }
}
