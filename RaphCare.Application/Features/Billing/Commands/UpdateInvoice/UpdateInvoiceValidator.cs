using FluentValidation;

namespace RaphCare.Application.Features.Billing.Commands.UpdateInvoice;

public class UpdateInvoiceValidator : AbstractValidator<UpdateInvoiceCommand>
{
    public UpdateInvoiceValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
