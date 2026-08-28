using Microsoft.Extensions.Logging;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Infrastructure.Services;

/// <summary>Payment processing integration. Placeholder for mobile money and card payments.</summary>
public sealed partial class PaymentGatewayService(ILogger<PaymentGatewayService> logger) : IPaymentGatewayService
{
    public Task<string> ChargeAsync(Guid patientId, decimal amount, string currency, CancellationToken cancellationToken = default)
    {
        LogPaymentPlaceholder(patientId, amount, currency);
        return Task.FromResult($"txn_{Guid.NewGuid():N}");
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

    [LoggerMessage(Level = LogLevel.Information, Message = "Refund placeholder: TxnId={TransactionId}, Amount={Amount}")]
    private partial void LogRefundPlaceholder(string transactionId, decimal amount);
}
