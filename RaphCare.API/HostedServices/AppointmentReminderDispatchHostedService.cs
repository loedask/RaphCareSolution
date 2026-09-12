using MediatR;
using RaphCare.Application.Features.Appointments.Commands.ProcessDueAppointmentReminders;

namespace RaphCare.API.HostedServices;

/// <summary>
/// Polls due appointment reminders every few minutes and sends in-app notices (and push when configured).
/// </summary>
public sealed partial class AppointmentReminderDispatchHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<AppointmentReminderDispatchHostedService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(2);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Brief delay so the host finishes warm-up before the first poll.
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                var sent = await mediator
                    .Send(new ProcessDueAppointmentRemindersCommand(), stoppingToken)
                    .ConfigureAwait(false);
                if (sent > 0)
                    LogDispatched(logger, sent);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                LogPollFailed(logger, ex);
            }

            try
            {
                await Task.Delay(Interval, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Dispatched {Count} appointment reminder(s).")]
    private static partial void LogDispatched(ILogger logger, int count);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Appointment reminder poll failed.")]
    private static partial void LogPollFailed(ILogger logger, Exception exception);
}
