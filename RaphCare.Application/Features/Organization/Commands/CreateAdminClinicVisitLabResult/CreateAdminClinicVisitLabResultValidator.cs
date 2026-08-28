using FluentValidation;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicVisitLabResult;

public sealed class CreateAdminClinicVisitLabResultValidator : AbstractValidator<CreateAdminClinicVisitLabResultCommand>
{
    public CreateAdminClinicVisitLabResultValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.VisitId).NotEmpty();
        RuleFor(x => x.TestName).NotEmpty().MaximumLength(256);
        RuleFor(x => x.ResultValue).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Unit).MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.Unit));
        RuleFor(x => x.ReferenceRange).MaximumLength(256).When(x => !string.IsNullOrWhiteSpace(x.ReferenceRange));
    }
}
