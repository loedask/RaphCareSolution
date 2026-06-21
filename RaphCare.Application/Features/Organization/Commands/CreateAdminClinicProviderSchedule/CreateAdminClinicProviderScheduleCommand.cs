using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicProviderSchedule;

public sealed class CreateAdminClinicProviderScheduleCommand : IRequest<AdminClinicProviderScheduleDto?>
{
    public Guid ClinicId { get; set; }
    public Guid ProviderId { get; set; }
    public DayOfWeek Day { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
}
