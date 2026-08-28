using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Integrations;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client.Services;

/// <summary>Posts standalone emergency webhook events on a no-bearer <see cref="ServiceRegistration.WebhookHttpClientName"/> client.</summary>
public sealed class StandaloneEmergencyWebhookClient(IHttpClientFactory httpClientFactory) : IStandaloneEmergencyWebhookClient
{
    public async Task<Response<IngestDeviceEmergencyEventResultViewModel>> PostEventAsync(
        StandaloneEmergencyWebhookRequest body,
        string? webhookSharedSecret,
        CancellationToken cancellationToken = default)
    {
        if (body is null)
            return Response<IngestDeviceEmergencyEventResultViewModel>.Failure("Body is required.");

        var command = new
        {
            serialNumber = body.SerialNumber,
            eventType = body.EventType,
            occurredAtUtc = body.OccurredAtUtc,
            externalEventId = body.ExternalEventId,
            latitude = body.Latitude,
            longitude = body.Longitude,
            horizontalAccuracyMeters = body.HorizontalAccuracyMeters
        };

        var http = httpClientFactory.CreateClient(ServiceRegistration.WebhookHttpClientName);
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/integrations/standalone-emergency/events")
        {
            Content = JsonContent.Create(command, options: ApiJson.Options)
        };

        var secret = webhookSharedSecret?.Trim();
        if (!string.IsNullOrEmpty(secret))
        {
            var utf8 = ApiJson.SerializeToUtf8Bytes(command);
            request.Headers.TryAddWithoutValidation("X-RaphCare-Emergency-Signature", ComputeHmacHex(secret, utf8));
        }

        try
        {
            using var response = await http.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                return Response<IngestDeviceEmergencyEventResultViewModel>.Failure(
                    string.IsNullOrEmpty(err) ? $"API error: {response.StatusCode}" : err,
                    (int)response.StatusCode);
            }

            var dto = await response.Content.ReadFromJsonAsync<IngestResultDto>(ApiJson.Options, cancellationToken).ConfigureAwait(false);
            if (dto is null)
                return Response<IngestDeviceEmergencyEventResultViewModel>.Failure("Empty response from server.");

            return Response<IngestDeviceEmergencyEventResultViewModel>.Success(
                new IngestDeviceEmergencyEventResultViewModel
                {
                    Id = dto.Id,
                    WasDuplicate = dto.WasDuplicate
                });
        }
        catch (HttpRequestException)
        {
            return Response<IngestDeviceEmergencyEventResultViewModel>.Failure(
                "We couldn't reach the server. Check your connection and try again.");
        }
    }

    private static string ComputeHmacHex(string secret, byte[] bodyUtf8)
    {
        var key = Encoding.UTF8.GetBytes(secret);
        using var hmac = new HMACSHA256(key);
        var hash = hmac.ComputeHash(bodyUtf8);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private sealed class IngestResultDto
    {
        public Guid Id { get; set; }
        public bool WasDuplicate { get; set; }
    }
}
