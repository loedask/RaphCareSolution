using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.UpdateAdminClinicReferralStatus;

public sealed class UpdateAdminClinicReferralStatusCommand : IRequest<AdminClinicReferralDto?>
{
    public Guid ClinicId { get; set; }
    public Guid ReferralId { get; set; }
    public string Status { get; set; } = string.Empty;
}
