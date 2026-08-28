using FluentValidation;

namespace RaphCare.Application.Features.Organization.Commands.UpdateAdminClinic;

public sealed class UpdateAdminClinicValidator : AbstractValidator<UpdateAdminClinicCommand>
{
    public UpdateAdminClinicValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(100);
        RuleFor(x => x.TimeZone).NotEmpty().MaximumLength(50);
    }
}
