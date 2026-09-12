using Microsoft.Extensions.Logging;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Infrastructure.Services;

/// <summary>Payment processing integration. Placeholder when Paystack is not configured.</summary>
public sealed partial class PaymentGatewayService(ILogger<PaymentGatewayService> logger) : IPaymentGatewayService
{
    public bool IsConfigured => false;

    public Task<string> ChargeAsync(Guid patientId, decimal amount, string currency, CancellationToken cancellationToken = default)
    {
        LogPaymentPlaceholder(patientId, amount, currency);
        return Task.FromResult($"txn_{Guid.NewGuid():N}");
    }

    public Task<PaymentCheckoutSession?> InitializeCheckoutAsync(
        Guid patientId,
        string customerEmail,
        decimal amount,
        string currency,
        string planCode,
        string? callbackUrl,
        CancellationToken cancellationToken = default)
    {
        LogCheckoutPlaceholder(patientId, planCode, amount, currency);
        return Task.FromResult<PaymentCheckoutSession?>(null);
    }

    public Task<PaymentChargeVerification> VerifyCheckoutAsync(string reference, CancellationToken cancellationToken = default)
    {
        LogVerifyPlaceholder(reference);
        return Task.FromResult(new PaymentChargeVerification
        {
            Succeeded = false,
            Reference = reference,
            GatewayMessage = "Payment gateway is not configured."
        });
    }

    public Task RefundAsync(string transactionId, decimal amount, CancellationToken cancellationToken = default)
    {
        LogRefundPlaceholder(transactionId, amount);
        return Task.CompletedTask;
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Payment placeholder: PatientId={PatientId}, Amount={Amount} {Currency}")]
    private partial void LogPaymentPlaceholder(Guid patientId, decimal amount, string currency);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Checkout placeholder: PatientId={PatientId}, Plan={PlanCode}, Amount={Amount} {Currency}")]
    private partial void LogCheckoutPlaceholder(Guid patientId, string planCode, decimal amount, string currency);

    [LoggerMessage(Level = LogLevel.Information, Message = "Verify placeholder: Reference={Reference}")]
    private partial void LogVerifyPlaceholder(string reference);

    [LoggerMessage(Level = LogLevel.Information, Message = "Refund placeholder: TxnId={TransactionId}, Amount={Amount}")]
    private partial void LogRefundPlaceholder(string transactionId, decimal amount);
}
