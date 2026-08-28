using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.TransferAdminClinicAdmission;

public sealed class TransferAdminClinicAdmissionCommand : IRequest<AdminClinicAdmissionDto?>
{
    public Guid ClinicId { get; set; }
    public Guid AdmissionId { get; set; }
    public Guid TargetBedId { get; set; }
    public string? Notes { get; set; }
}
