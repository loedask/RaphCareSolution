using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaphCare.Application.Common.Configuration;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Infrastructure.Services;

/// <summary>Paystack initialize + verify for patient care plan checkouts.</summary>
public sealed partial class PaystackPaymentGatewayService(
    IHttpClientFactory httpClientFactory,
    IOptions<PaystackOptions> options,
    ILogger<PaystackPaymentGatewayService> logger) : IPaymentGatewayService
{
    public const string HttpClientName = "Paystack";

    private readonly PaystackOptions _options = options.Value;

    public bool IsConfigured => _options.IsEnabled;

    public async Task<string> ChargeAsync(
        Guid patientId,
        decimal amount,
        string currency,
        CancellationToken cancellationToken = default)
    {
        // Server-side one-shot charge without a card token is not used; use InitializeCheckoutAsync.
        LogChargeSkipped(patientId, amount, currency);
        return $"paystack_pending_{Guid.NewGuid():N}";
    }

    public async Task<PaymentCheckoutSession?> InitializeCheckoutAsync(
        Guid patientId,
        string customerEmail,
        decimal amount,
        string currency,
        string planCode,
        string? callbackUrl,
        CancellationToken cancellationToken = default)
    {
        if (!_options.IsEnabled)
            return null;

        var reference = $"rc{Guid.NewGuid():N}";
        // Paystack expects amount in the smallest currency unit (kobo/cents). ZAR/USD use 100.
        var amountMinor = (int)Math.Round(amount * 100m, MidpointRounding.AwayFromZero);
        var payload = new
        {
            email = customerEmail,
            amount = amountMinor,
            currency = string.IsNullOrWhiteSpace(currency) ? "ZAR" : currency.Trim().ToUpperInvariant(),
            reference,
            callback_url = string.IsNullOrWhiteSpace(callbackUrl) ? _options.CallbackUrl : callbackUrl,
            metadata = new
            {
                patient_id = patientId.ToString("D"),
                plan_code = planCode.Trim().ToUpperInvariant(),
                custom_fields = new[]
                {
                    new { display_name = "Plan", variable_name = "plan_code", value = planCode.Trim().ToUpperInvariant() }
                }
            }
        };

        var client = httpClientFactory.CreateClient(HttpClientName);
        using var response = await client.PostAsJsonAsync("transaction/initialize", payload, cancellationToken)
            .ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            LogPaystackHttpError("initialize", (int)response.StatusCode, body);
            throw new InvalidOperationException("Paystack could not start checkout.");
        }

        var parsed = JsonSerializer.Deserialize<PaystackInitializeResponse>(body, JsonOptions);
        if (parsed?.Status != true || parsed.Data is null || string.IsNullOrWhiteSpace(parsed.Data.AuthorizationUrl))
            throw new InvalidOperationException(parsed?.Message ?? "Paystack initialize failed.");

        return new PaymentCheckoutSession
        {
            Reference = parsed.Data.Reference ?? reference,
            AuthorizationUrl = parsed.Data.AuthorizationUrl,
            AccessCode = parsed.Data.AccessCode
        };
    }

    public async Task<PaymentChargeVerification> VerifyCheckoutAsync(
        string reference,
        CancellationToken cancellationToken = default)
    {
        if (!_options.IsEnabled)
        {
            return new PaymentChargeVerification
            {
                Succeeded = false,
                Reference = reference,
                GatewayMessage = "Paystack is not configured."
            };
        }

        var client = httpClientFactory.CreateClient(HttpClientName);
        using var response = await client.GetAsync(
                $"transaction/verify/{Uri.EscapeDataString(reference)}",
                cancellationToken)
            .ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            LogPaystackHttpError("verify", (int)response.StatusCode, body);
            return new PaymentChargeVerification
            {
                Succeeded = false,
                Reference = reference,
                GatewayMessage = "Paystack verify failed."
            };
        }

        var parsed = JsonSerializer.Deserialize<PaystackVerifyResponse>(body, JsonOptions);
        var data = parsed?.Data;
        var ok = parsed?.Status == true
            && string.Equals(data?.Status, "success", StringComparison.OrdinalIgnoreCase);

        Guid? patientId = null;
        if (Guid.TryParse(data?.Metadata?.PatientId, out var pid))
            patientId = pid;

        return new PaymentChargeVerification
        {
            Succeeded = ok,
            Reference = data?.Reference ?? reference,
            Amount = data is null ? 0 : data.Amount / 100m,
            Currency = data?.Currency ?? "ZAR",
            PlanCode = data?.Metadata?.PlanCode,
            PatientId = patientId,
            GatewayMessage = parsed?.Message
        };
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private sealed class PaystackInitializeResponse
    {
        public bool Status { get; set; }
        public string? Message { get; set; }
        public PaystackInitializeData? Data { get; set; }
    }

    private sealed class PaystackInitializeData
    {
        [JsonPropertyName("authorization_url")]
        public string? AuthorizationUrl { get; set; }

        [JsonPropertyName("access_code")]
        public string? AccessCode { get; set; }

        public string? Reference { get; set; }
    }

    private sealed class PaystackVerifyResponse
    {
        public bool Status { get; set; }
        public string? Message { get; set; }
        public PaystackVerifyData? Data { get; set; }
    }

    private sealed class PaystackVerifyData
    {
        public string? Status { get; set; }
        public string? Reference { get; set; }
        public int Amount { get; set; }
        public string? Currency { get; set; }
        public PaystackMetadata? Metadata { get; set; }
    }

    private sealed class PaystackMetadata
    {
        [JsonPropertyName("patient_id")]
        public string? PatientId { get; set; }

        [JsonPropertyName("plan_code")]
        public string? PlanCode { get; set; }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Paystack ChargeAsync skipped for PatientId={PatientId} Amount={Amount} {Currency}")]
    private partial void LogChargeSkipped(Guid patientId, decimal amount, string currency);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Paystack {Operation} HTTP {StatusCode}: {Body}")]
    private partial void LogPaystackHttpError(string operation, int statusCode, string body);
}
