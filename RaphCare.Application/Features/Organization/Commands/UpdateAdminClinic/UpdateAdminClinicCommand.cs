using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.UpdateAdminClinic;

public sealed class UpdateAdminClinicCommand : IRequest<ClinicDetailDto?>
{
    public Guid ClinicId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public string TimeZone { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public bool AllowAiDischargeDraft { get; init; }
}
