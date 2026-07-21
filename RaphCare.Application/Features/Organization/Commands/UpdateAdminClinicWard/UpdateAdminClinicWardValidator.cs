using FluentValidation;

namespace RaphCare.Application.Features.Organization.Commands.UpdateAdminClinicWard;

public sealed class UpdateAdminClinicWardValidator : AbstractValidator<UpdateAdminClinicWardCommand>
{
    public UpdateAdminClinicWardValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.WardId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Code).MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.Code));
    }
}
