using MediatR;

namespace RaphCare.Application.Features.Organization.Commands.SetAdminClinicProviderActive;

public sealed class SetAdminClinicProviderActiveCommand : IRequest<bool>
{
    public Guid ClinicId { get; set; }
    public Guid ProviderId { get; set; }
    public bool IsActive { get; set; }
}
