using FluentValidation;

namespace RaphCare.Application.Features.Organization.Commands.CompleteAdminClinicLabOrder;

public sealed class CompleteAdminClinicLabOrderValidator : AbstractValidator<CompleteAdminClinicLabOrderCommand>
{
    public CompleteAdminClinicLabOrderValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.LabRequestId).NotEmpty();
        RuleFor(x => x.ResultValue).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Unit).MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.Unit));
        RuleFor(x => x.ReferenceRange).MaximumLength(256).When(x => !string.IsNullOrWhiteSpace(x.ReferenceRange));
    }
}
