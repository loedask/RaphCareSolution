using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Clinical;

namespace RaphCare.Application.Features.Appointments;

/// <summary>
/// Schedules T-24h and T-2h in-app reminder rows when those times are still in the future.
/// </summary>
public static class AppointmentReminderPlanner
{
    private static readonly TimeSpan[] OffsetsBeforeStart =
    [
        TimeSpan.FromHours(24),
        TimeSpan.FromHours(2)
    ];

    public static async Task ReplaceUnsentAsync(
        IRepository<AppointmentReminder> reminderRepository,
        Guid appointmentId,
        DateTime scheduledStartUtc,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        await ClearUnsentAsync(reminderRepository, appointmentId, cancellationToken).ConfigureAwait(false);

        foreach (var offset in OffsetsBeforeStart)
        {
            var reminderTime = scheduledStartUtc - offset;
            if (reminderTime <= utcNow)
                continue;

            await reminderRepository.AddAsync(
                    new AppointmentReminder
                    {
                        AppointmentId = appointmentId,
                        ReminderTime = reminderTime,
                        Channel = AppointmentReminderChannels.InApp,
                        Sent = false
                    },
                    cancellationToken)
                .ConfigureAwait(false);
        }
    }

    public static async Task ClearUnsentAsync(
        IRepository<AppointmentReminder> reminderRepository,
        Guid appointmentId,
        CancellationToken cancellationToken)
    {
        var page = await reminderRepository.SearchAsync(
                q => q.Where(r => r.AppointmentId == appointmentId && !r.Sent),
                1,
                50,
                applyDefaultIdOrdering: false,
                cancellationToken)
            .ConfigureAwait(false);

        foreach (var reminder in page.Items)
            await reminderRepository.DeleteAsync(reminder, cancellationToken).ConfigureAwait(false);
    }
}
