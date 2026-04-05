using MediatR;

namespace RaphCare.Application.Features.PatientBilling.Commands.RemoveMyPaymentMethod;

public sealed class RemoveMyPaymentMethodCommand : IRequest<Unit>
{
    public Guid PaymentMethodId { get; set; }
}
