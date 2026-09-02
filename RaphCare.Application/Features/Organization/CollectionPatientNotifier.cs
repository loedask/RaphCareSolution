using MediatR;
using RaphCare.Application.Features.PatientNotifications.Commands.CreatePatientInAppNotification;

namespace RaphCare.Application.Features.Organization;

internal static class CollectionPatientNotifier
{
    public static async Task NotifyReadyAsync(
        IMediator mediator,
        Guid patientId,
        string clinicName,
        string pickupCode,
        bool isLab,
        CancellationToken cancellationToken)
    {
        var place = string.IsNullOrWhiteSpace(clinicName) ? "the hospital" : clinicName.Trim();
        var title = isLab ? "Lab test ready to collect" : "Prescription ready to collect";
        var body = $"Show this pickup code at {place}: {pickupCode}.";
        try
        {
            await mediator.Send(
                    new CreatePatientInAppNotificationCommand
                    {
                        PatientId = patientId,
                        Title = title,
                        Body = body,
                        Type = "collection",
                        SendPush = true
                    },
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception)
        {
            // The order is already saved. A failed notice must not fail the write.
        }
    }

    public static async Task NotifyCalledAsync(
        IMediator mediator,
        Guid patientId,
        string clinicName,
        string pickupCode,
        bool isLab,
        CancellationToken cancellationToken)
    {
        var place = isLab ? "the lab" : "the pharmacy";
        if (!string.IsNullOrWhiteSpace(clinicName))
            place = isLab ? $"the lab at {clinicName.Trim()}" : $"the pharmacy at {clinicName.Trim()}";

        var title = isLab ? "Come to the lab" : "Come to the pharmacy";
        var body = $"Come to {place}. Your code is {pickupCode}.";
        try
        {
            await mediator.Send(
                    new CreatePatientInAppNotificationCommand
                    {
                        PatientId = patientId,
                        Title = title,
                        Body = body,
                        Type = "collection",
                        SendPush = true
                    },
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception)
        {
            // The call is already saved. A failed notice must not fail the write.
        }
    }

    public static async Task NotifyLabResultReadyAsync(
        IMediator mediator,
        Guid patientId,
        string clinicName,
        string testName,
        CancellationToken cancellationToken)
    {
        var place = string.IsNullOrWhiteSpace(clinicName) ? "the hospital" : clinicName.Trim();
        var test = string.IsNullOrWhiteSpace(testName) ? "Your lab test" : testName.Trim();
        var title = "Your lab result is ready";
        var body = $"{test} is ready to view in Health records at {place}.";
        try
        {
            await mediator.Send(
                    new CreatePatientInAppNotificationCommand
                    {
                        PatientId = patientId,
                        Title = title,
                        Body = body,
                        Type = "lab",
                        SendPush = true
                    },
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception)
        {
            // The result is already saved. A failed notice must not fail the write.
        }
    }
}
