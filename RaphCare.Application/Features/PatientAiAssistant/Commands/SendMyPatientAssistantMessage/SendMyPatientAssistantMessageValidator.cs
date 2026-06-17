using FluentValidation;

namespace RaphCare.Application.Features.PatientAiAssistant.Commands.SendMyPatientAssistantMessage;

public sealed class SendMyPatientAssistantMessageValidator : AbstractValidator<SendMyPatientAssistantMessageCommand>
{
    public SendMyPatientAssistantMessageValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty()
            .Must(m => !string.IsNullOrWhiteSpace(m))
            .MaximumLength(2000)
            .Must(m => !m.Contains('\0'))
            .WithMessage("Message contains invalid characters.");
    }
}
