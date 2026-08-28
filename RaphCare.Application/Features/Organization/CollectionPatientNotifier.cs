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
}
