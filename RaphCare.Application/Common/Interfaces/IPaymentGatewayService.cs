namespace RaphCare.Application.Common.Interfaces;

/// <summary>Result of starting a hosted checkout with the payment provider.</summary>
public sealed class PaymentCheckoutSession
{
    public required string Reference { get; init; }
    public required string AuthorizationUrl { get; init; }
    public string? AccessCode { get; init; }
}

/// <summary>Verified charge from the payment provider.</summary>
public sealed class PaymentChargeVerification
{
    public required bool Succeeded { get; init; }
    public required string Reference { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "ZAR";
    public string? PlanCode { get; init; }
    public Guid? PatientId { get; init; }
    public string? GatewayMessage { get; init; }
}

public interface IPaymentGatewayService
{
    /// <summary>True when a live provider (for example Paystack) is configured.</summary>
    bool IsConfigured { get; }

    Task<string> ChargeAsync(Guid patientId, decimal amount, string currency, CancellationToken cancellationToken = default);

    /// <summary>Starts a hosted checkout. Returns null when the gateway is not configured.</summary>
    Task<PaymentCheckoutSession?> InitializeCheckoutAsync(
        Guid patientId,
        string customerEmail,
        decimal amount,
        string currency,
        string planCode,
        string? callbackUrl,
        CancellationToken cancellationToken = default);

    /// <summary>Verifies a charge by provider reference.</summary>
    Task<PaymentChargeVerification> VerifyCheckoutAsync(string reference, CancellationToken cancellationToken = default);
}
