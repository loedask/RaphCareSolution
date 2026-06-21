using MediatR;

namespace RaphCare.Application.Features.Organization.Commands.DeleteAdminClinicProviderSchedule;

public sealed class DeleteAdminClinicProviderScheduleCommand : IRequest<bool>
{
    public Guid ClinicId { get; set; }
    public Guid ProviderId { get; set; }
    public Guid ScheduleId { get; set; }
}
