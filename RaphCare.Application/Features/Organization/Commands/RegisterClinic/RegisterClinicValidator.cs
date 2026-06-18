using FluentValidation;

namespace RaphCare.Application.Features.Organization.Commands.RegisterClinic;

public sealed class RegisterClinicValidator : AbstractValidator<RegisterClinicCommand>
{
    public RegisterClinicValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.RegistrationNumber).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(100);
        RuleFor(x => x.TimeZone).NotEmpty().MaximumLength(50);

        When(x => !string.IsNullOrWhiteSpace(x.FacilityName), () =>
        {
            RuleFor(x => x.FacilityName).MaximumLength(200);
            RuleFor(x => x.FacilityAddress).MaximumLength(500);
            RuleFor(x => x.FacilityCity).MaximumLength(100);
        });
    }
}
