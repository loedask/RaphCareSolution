namespace RaphCare.Application.Common.Interfaces;

public interface IPaymentGatewayService
{
    Task<string> ChargeAsync(Guid patientId, decimal amount, string currency, CancellationToken cancellationToken = default);
}

