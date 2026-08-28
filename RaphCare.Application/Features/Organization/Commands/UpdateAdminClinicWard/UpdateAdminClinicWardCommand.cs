using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.UpdateAdminClinicWard;

public sealed class UpdateAdminClinicWardCommand : IRequest<AdminClinicWardDto?>
{
    public Guid ClinicId { get; set; }
    public Guid WardId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public bool IsActive { get; set; } = true;
}
