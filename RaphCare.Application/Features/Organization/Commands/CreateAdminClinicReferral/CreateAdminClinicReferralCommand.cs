using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicReferral;

public sealed class CreateAdminClinicReferralCommand : IRequest<AdminClinicReferralDto?>
{
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    public Guid? VisitId { get; set; }
    public string ReferredTo { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string? Specialty { get; set; }
    public string? Notes { get; set; }
}
