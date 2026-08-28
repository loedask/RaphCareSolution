using FluentValidation;

namespace RaphCare.Application.Features.PatientSupport.Commands.SubmitPatientSupportMessage;

public sealed class SubmitPatientSupportMessageValidator : AbstractValidator<SubmitPatientSupportMessageCommand>
{
    public SubmitPatientSupportMessageValidator()
    {
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Message).NotEmpty().MaximumLength(4000);
    }
}
