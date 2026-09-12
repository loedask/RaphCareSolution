using MediatR;

namespace RaphCare.Application.Features.Appointments.Commands.ProcessDueAppointmentReminders;

/// <summary>Dispatches due in-app appointment reminders. Intended for the API hosted poller.</summary>
public sealed class ProcessDueAppointmentRemindersCommand : IRequest<int>
{
    public int BatchSize { get; set; } = 50;
}
