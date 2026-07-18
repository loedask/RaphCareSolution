using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.DischargeAdminClinicAdmission;

public sealed class DischargeAdminClinicAdmissionCommand : IRequest<AdminClinicAdmissionDto?>
{
    public Guid ClinicId { get; set; }
    public Guid AdmissionId { get; set; }
    public string? Notes { get; set; }
}
