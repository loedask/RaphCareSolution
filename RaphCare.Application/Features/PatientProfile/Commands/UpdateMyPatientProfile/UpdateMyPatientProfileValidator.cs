using FluentValidation;

namespace RaphCare.Application.Features.PatientProfile.Commands.UpdateMyPatientProfile;

public sealed class UpdateMyPatientProfileValidator : AbstractValidator<UpdateMyPatientProfileCommand>
{
    public UpdateMyPatientProfileValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).MaximumLength(256).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.PhoneNumber).MaximumLength(30);
        RuleFor(x => x.Gender).NotEmpty().MaximumLength(20);
        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.UtcNow.Date.AddDays(1))
            .WithMessage("Date of birth cannot be in the future.");
        RuleFor(x => x.DateOfBirth)
            .GreaterThan(DateTime.UtcNow.Date.AddYears(-130))
            .WithMessage("Date of birth is not valid.");
    }
}
