using FluentValidation;

namespace RaphCare.Application.Features.PatientTelehealth.Commands.SendTelehealthChatMessage;

public sealed class SendTelehealthChatMessageValidator : AbstractValidator<SendTelehealthChatMessageCommand>
{
    public SendTelehealthChatMessageValidator()
    {
        RuleFor(x => x.TeleSessionId).NotEmpty();
        RuleFor(x => x.Message).NotEmpty().MaximumLength(2000);
    }
}
