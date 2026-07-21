using MediatR;

namespace RaphCare.Application.Features.Organization.Commands.DeleteAdminClinicWard;

public sealed class DeleteAdminClinicWardCommand : IRequest<bool>
{
    public Guid ClinicId { get; set; }
    public Guid WardId { get; set; }
}
