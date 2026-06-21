using FluentValidation;

namespace RaphCare.Application.Features.PatientEmergencyContacts.Commands.UpdateMyPatientEmergencyContact;

public sealed class UpdateMyPatientEmergencyContactValidator : AbstractValidator<UpdateMyPatientEmergencyContactCommand>
{
    public UpdateMyPatientEmergencyContactValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Relationship).MaximumLength(100).When(x => x.Relationship != null);
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Email).MaximumLength(256).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}
