using FluentValidation;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicVisitVital;

public sealed class CreateAdminClinicVisitVitalValidator : AbstractValidator<CreateAdminClinicVisitVitalCommand>
{
    public CreateAdminClinicVisitVitalValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.VisitId).NotEmpty();
        RuleFor(x => x.Type).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Unit).MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.Unit));
    }
}
