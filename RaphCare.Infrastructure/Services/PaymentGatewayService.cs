using Microsoft.Extensions.Logging;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.Infrastructure.Services;

public class PaymentGatewayService : IPaymentGatewayService
{
    private readonly ILogger<PaymentGatewayService> _logger;

    public PaymentGatewayService(ILogger<PaymentGatewayService> logger)
    {
        _logger = logger;
    }

    public Task<string> ChargeAsync(Guid patientId, decimal amount, string currency, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Payment placeholder: PatientId={PatientId}, Amount={Amount} {Currency}", patientId, amount, currency);
        return Task.FromResult($"txn_{Guid.NewGuid():N}");
    }

    public Task RefundAsync(string transactionId, decimal amount, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Refund placeholder: TxnId={TransactionId}, Amount={Amount}", transactionId, amount);
        return Task.CompletedTask;
    }
}
