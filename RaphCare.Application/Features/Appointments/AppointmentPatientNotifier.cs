using System.Globalization;
using MediatR;
using RaphCare.Application.Features.PatientNotifications.Commands.CreatePatientInAppNotification;

namespace RaphCare.Application.Features.Appointments;

internal static class AppointmentPatientNotifier
{
    public static async Task NotifyBookedAsync(
        IMediator mediator,
        Guid patientId,
        string clinicName,
        DateTime scheduledStartUtc,
        CancellationToken cancellationToken)
    {
        var place = string.IsNullOrWhiteSpace(clinicName) ? "your clinic" : clinicName.Trim();
        var when = scheduledStartUtc.ToString("dd MMM yyyy HH:mm", CultureInfo.InvariantCulture) + " UTC";
        await SendSafeAsync(
                mediator,
                patientId,
                "Appointment booked",
                $"Your visit at {place} is scheduled for {when}.",
                cancellationToken)
            .ConfigureAwait(false);
    }

    public static async Task NotifyRescheduledAsync(
        IMediator mediator,
        Guid patientId,
        string clinicName,
        DateTime scheduledStartUtc,
        CancellationToken cancellationToken)
    {
        var place = string.IsNullOrWhiteSpace(clinicName) ? "your clinic" : clinicName.Trim();
        var when = scheduledStartUtc.ToString("dd MMM yyyy HH:mm", CultureInfo.InvariantCulture) + " UTC";
        await SendSafeAsync(
                mediator,
                patientId,
                "Appointment changed",
                $"Your visit at {place} was moved to {when}.",
                cancellationToken)
            .ConfigureAwait(false);
    }

    public static async Task NotifyUpcomingAsync(
        IMediator mediator,
        Guid patientId,
        string clinicName,
        DateTime scheduledStartUtc,
        CancellationToken cancellationToken)
    {
        var place = string.IsNullOrWhiteSpace(clinicName) ? "your clinic" : clinicName.Trim();
        var when = scheduledStartUtc.ToString("dd MMM yyyy HH:mm", CultureInfo.InvariantCulture) + " UTC";
        await SendSafeAsync(
                mediator,
                patientId,
                "Upcoming appointment",
                $"Reminder: your visit at {place} is at {when}.",
                cancellationToken)
            .ConfigureAwait(false);
    }

    private static async Task SendSafeAsync(
        IMediator mediator,
        Guid patientId,
        string title,
        string body,
        CancellationToken cancellationToken)
    {
        try
        {
            await mediator.Send(
                    new CreatePatientInAppNotificationCommand
                    {
                        PatientId = patientId,
                        Title = title,
                        Body = body,
                        Type = "appointment",
                        SendPush = true
                    },
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception)
        {
            // Appointment write already succeeded. A failed notice must not fail the write.
        }
    }
}
