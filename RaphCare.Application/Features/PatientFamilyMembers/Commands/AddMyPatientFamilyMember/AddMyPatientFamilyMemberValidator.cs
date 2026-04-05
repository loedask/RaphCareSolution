using FluentValidation;

namespace RaphCare.Application.Features.PatientFamilyMembers.Commands.AddMyPatientFamilyMember;

public sealed class AddMyPatientFamilyMemberValidator : AbstractValidator<AddMyPatientFamilyMemberCommand>
{
    public AddMyPatientFamilyMemberValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Relationship).NotEmpty().MaximumLength(50);
        RuleFor(x => x.PhoneNumber).MaximumLength(30).When(x => x.PhoneNumber != null);
        RuleFor(x => x.Email).MaximumLength(256).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.UtcNow.Date.AddDays(1))
            .When(x => x.DateOfBirth.HasValue)
            .WithMessage("Date of birth cannot be in the future.");
        RuleFor(x => x.LinkedPatientId)
            .Must(id => id is null || id != Guid.Empty)
            .WithMessage("Linked patient id must be a non-empty guid when set.");
    }
}
