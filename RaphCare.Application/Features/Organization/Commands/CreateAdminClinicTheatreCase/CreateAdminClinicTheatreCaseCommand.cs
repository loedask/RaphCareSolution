using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicTheatreCase;

public sealed class CreateAdminClinicTheatreCaseCommand : IRequest<AdminClinicTheatreCaseDto?>
{
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    public DateTime ScheduledStart { get; set; }
    public DateTime? ScheduledEnd { get; set; }
    public string ProcedureName { get; set; } = string.Empty;
    public string? TheatreName { get; set; }
    public string? SurgeonName { get; set; }
    public string? Notes { get; set; }
}
