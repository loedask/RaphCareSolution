using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using RaphCare.Client.Contracts;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Integrations;

namespace RaphCare.Client.Services;

/// <summary>Hand-rolled POST for standalone emergency ingestion (correct JSON body + HMAC). See <see cref="IStandaloneEmergencyWebhookClient"/>.</summary>
public sealed class StandaloneEmergencyWebhookClient(IHttpClientFactory httpClientFactory) : IStandaloneEmergencyWebhookClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly JsonSerializerOptions ReadOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public async Task<Response<IngestDeviceEmergencyEventResultViewModel>> PostEventAsync(
        StandaloneEmergencyWebhookRequest body,
        string? webhookSharedSecret,
        CancellationToken cancellationToken = default)
    {
        if (body is null)
            return Response<IngestDeviceEmergencyEventResultViewModel>.Failure("Body is required.");

        var client = _httpClientFactory.CreateClient(ServiceRegistration.WebhookHttpClientName);
        var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(body, JsonOptions);

        using var content = new ByteArrayContent(jsonBytes);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json") { CharSet = "utf-8" };

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/integrations/standalone-emergency/events")
        {
            Content = content
        };

        var secret = webhookSharedSecret?.Trim();
        if (!string.IsNullOrEmpty(secret))
        {
            var hex = ComputeHmacHex(secret, jsonBytes);
            request.Headers.TryAddWithoutValidation("X-RaphCare-Emergency-Signature", hex);
        }

        using var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var text = response.Content is null
            ? string.Empty
            : await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            var vm = JsonSerializer.Deserialize<IngestDeviceEmergencyEventResultViewModel>(text, ReadOptions);
            if (vm is null)
                return Response<IngestDeviceEmergencyEventResultViewModel>.Failure("Empty response.", (int)response.StatusCode);

            return Response<IngestDeviceEmergencyEventResultViewModel>.Success(vm);
        }

        return Response<IngestDeviceEmergencyEventResultViewModel>.Failure(
            string.IsNullOrWhiteSpace(text) ? response.ReasonPhrase ?? "Request failed." : text,
            (int)response.StatusCode);
    }

    private static string ComputeHmacHex(string secret, byte[] bodyUtf8)
    {
        var key = Encoding.UTF8.GetBytes(secret);
        using var hmac = new HMACSHA256(key);
        var hash = hmac.ComputeHash(bodyUtf8);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
