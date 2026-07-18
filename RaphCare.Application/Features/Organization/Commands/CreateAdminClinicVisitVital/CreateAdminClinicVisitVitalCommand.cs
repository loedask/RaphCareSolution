using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicVisitVital;

public sealed class CreateAdminClinicVisitVitalCommand : IRequest<AdminClinicVisitVitalDto?>
{
    public Guid ClinicId { get; set; }
    public Guid VisitId { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string? Unit { get; set; }
    public DateTime? RecordedAt { get; set; }
}
