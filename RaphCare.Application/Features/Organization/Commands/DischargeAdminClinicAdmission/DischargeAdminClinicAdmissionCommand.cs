using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.DischargeAdminClinicAdmission;

public sealed class DischargeAdminClinicAdmissionCommand : IRequest<AdminClinicAdmissionDto?>
{
    public Guid ClinicId { get; set; }
    public Guid AdmissionId { get; set; }
    public string? Notes { get; set; }
    public string? DischargeSummary { get; set; }
    public decimal? NightlyBedRate { get; set; }
    public decimal? ExtraAmount { get; set; }
    public string? ExtraDescription { get; set; }
    public bool MarkPaid { get; set; }
    public string? Currency { get; set; }
}
