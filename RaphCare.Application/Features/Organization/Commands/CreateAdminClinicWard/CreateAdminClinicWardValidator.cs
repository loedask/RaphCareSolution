using FluentValidation;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicWard;

public sealed class CreateAdminClinicWardValidator : AbstractValidator<CreateAdminClinicWardCommand>
{
    public CreateAdminClinicWardValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.FacilityId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Code).MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.Code));
    }
}
