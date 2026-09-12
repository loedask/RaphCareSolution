using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Appointments.Commands.ProcessDueAppointmentReminders;

public sealed class ProcessDueAppointmentRemindersHandler(
    IRepository<AppointmentReminder> reminderRepository,
    IRepository<Appointment> appointmentRepository,
    IRepository<Clinic> clinicRepository,
    IDateTimeProvider clock,
    IMediator mediator,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ProcessDueAppointmentRemindersCommand, int>
{
    public async Task<int> Handle(ProcessDueAppointmentRemindersCommand request, CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        var batchSize = request.BatchSize <= 0 ? 50 : Math.Min(request.BatchSize, 200);

        var due = await reminderRepository.SearchAsync(
                q => q.Where(r =>
                        !r.Sent
                        && r.Channel == AppointmentReminderChannels.InApp
                        && r.ReminderTime <= now)
                    .OrderBy(r => r.ReminderTime),
                1,
                batchSize,
                applyDefaultIdOrdering: false,
                cancellationToken)
            .ConfigureAwait(false);

        var sent = 0;
        foreach (var reminder in due.Items)
        {
            var appointment = await appointmentRepository
                .GetByIdAsync(reminder.AppointmentId, cancellationToken)
                .ConfigureAwait(false);

            reminder.Sent = true;

            if (appointment is null || appointment.IsCancelled
                || string.Equals(appointment.Status, "Cancelled", StringComparison.OrdinalIgnoreCase)
                || string.Equals(appointment.Status, "Completed", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var clinic = await clinicRepository.GetByIdAsync(appointment.ClinicId, cancellationToken)
                .ConfigureAwait(false);
            await AppointmentPatientNotifier.NotifyUpcomingAsync(
                    mediator,
                    appointment.PatientId,
                    clinic?.Name ?? string.Empty,
                    appointment.ScheduledStart,
                    cancellationToken)
                .ConfigureAwait(false);
            sent++;
        }

        if (due.Items.Count > 0)
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return sent;
    }
}
