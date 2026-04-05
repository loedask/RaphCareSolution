using RaphCare.Client.Contracts;
using RaphCare.Client.Models.Integrations;

namespace RaphCare.Client.Contracts.Interfaces;

/// <summary>
/// Posts cellular / OEM emergency webhooks with optional HMAC (<c>X-RaphCare-Emergency-Signature</c>).
/// Uses a dedicated HttpClient without bearer auth and the generated <c>IClient.EventsAsync</c> contract.
/// </summary>
public interface IStandaloneEmergencyWebhookClient
{
    Task<Response<IngestDeviceEmergencyEventResultViewModel>> PostEventAsync(
        StandaloneEmergencyWebhookRequest body,
        string? webhookSharedSecret,
        CancellationToken cancellationToken = default);
}
