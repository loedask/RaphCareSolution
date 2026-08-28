using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicAppointment;

public sealed class CreateAdminClinicAppointmentCommand : IRequest<AdminClinicAppointmentListItemDto?>
{
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    public Guid ProviderId { get; set; }
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public string Type { get; set; } = "InPerson";
    public string? Reason { get; set; }
}
