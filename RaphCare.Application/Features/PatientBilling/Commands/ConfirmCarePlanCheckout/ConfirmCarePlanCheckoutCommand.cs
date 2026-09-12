using MediatR;

namespace RaphCare.Application.Features.PatientBilling.Commands.ConfirmCarePlanCheckout;

public sealed class ConfirmCarePlanCheckoutCommand : IRequest<Unit>
{
    public string Reference { get; set; } = string.Empty;
}
