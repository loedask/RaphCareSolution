using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.StartAdminClinicVisit;

public sealed class StartAdminClinicVisitCommand : IRequest<AdminClinicVisitDetailDto?>
{
    public Guid ClinicId { get; set; }
    public Guid AppointmentId { get; set; }
    public string? Summary { get; set; }
}
