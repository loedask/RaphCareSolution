using System.Security.Cryptography;
using System.Text;
using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Integrations;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client.Services;

/// <summary>Wraps generated <see cref="IClient.EventsAsync"/> on a no-bearer <see cref="ServiceRegistration.WebhookHttpClientName"/> client.</summary>
public sealed class StandaloneEmergencyWebhookClient(IHttpClientFactory httpClientFactory) : IStandaloneEmergencyWebhookClient
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public async Task<Response<IngestDeviceEmergencyEventResultViewModel>> PostEventAsync(
        StandaloneEmergencyWebhookRequest body,
        string? webhookSharedSecret,
        CancellationToken cancellationToken = default)
    {
        if (body is null)
            return Response<IngestDeviceEmergencyEventResultViewModel>.Failure("Body is required.");

        var http = _httpClientFactory.CreateClient(ServiceRegistration.WebhookHttpClientName);
        var client = new Client(http);

        var command = new IngestDeviceEmergencyEventCommand
        {
            SerialNumber = body.SerialNumber,
            EventType = body.EventType,
            OccurredAtUtc = body.OccurredAtUtc,
            ExternalEventId = body.ExternalEventId,
            Latitude = body.Latitude,
            Longitude = body.Longitude,
            HorizontalAccuracyMeters = body.HorizontalAccuracyMeters
        };

        var secret = webhookSharedSecret?.Trim();
        string? signatureHeader = null;
        if (!string.IsNullOrEmpty(secret))
        {
            var utf8 = client.SerializeRequestBodyToUtf8Bytes(command);
            signatureHeader = ComputeHmacHex(secret, utf8);
        }

        try
        {
            var dto = await client.EventsAsync(command, signatureHeader, cancellationToken).ConfigureAwait(false);
            return Response<IngestDeviceEmergencyEventResultViewModel>.Success(
                new IngestDeviceEmergencyEventResultViewModel
                {
                    Id = dto.Id,
                    WasDuplicate = dto.WasDuplicate
                });
        }
        catch (ApiException ex)
        {
            return Response<IngestDeviceEmergencyEventResultViewModel>.Failure(ex.Message, ex.StatusCode);
        }
    }

    private static string ComputeHmacHex(string secret, byte[] bodyUtf8)
    {
        var key = Encoding.UTF8.GetBytes(secret);
        using var hmac = new HMACSHA256(key);
        var hash = hmac.ComputeHash(bodyUtf8);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
