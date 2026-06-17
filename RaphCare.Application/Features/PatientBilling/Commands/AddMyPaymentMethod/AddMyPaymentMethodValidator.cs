using FluentValidation;

namespace RaphCare.Application.Features.PatientBilling.Commands.AddMyPaymentMethod;

public sealed class AddMyPaymentMethodValidator : AbstractValidator<AddMyPaymentMethodCommand>
{
    public AddMyPaymentMethodValidator()
    {
        RuleFor(x => x.MethodType).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ProviderName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.MaskedDetails).NotEmpty().MaximumLength(256);
    }
}
