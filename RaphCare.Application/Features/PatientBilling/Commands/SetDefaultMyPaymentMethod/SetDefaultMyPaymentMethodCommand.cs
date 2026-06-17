using MediatR;

namespace RaphCare.Application.Features.PatientBilling.Commands.SetDefaultMyPaymentMethod;

public sealed class SetDefaultMyPaymentMethodCommand : IRequest<Unit>
{
    public Guid PaymentMethodId { get; set; }
}
