using FluentValidation;
using RaphCare.Application.Features.PatientAiAssistant;

namespace RaphCare.Application.Features.PatientAiAssistant.Commands.SendMyPatientAssistantMessage;

public sealed class SendMyPatientAssistantMessageValidator : AbstractValidator<SendMyPatientAssistantMessageCommand>
{
    public SendMyPatientAssistantMessageValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty()
            .Must(m => !string.IsNullOrWhiteSpace(m))
            .MaximumLength(PatientAssistantChatHistoryRules.MaxContentLength)
            .Must(m => !m.Contains('\0'))
            .WithMessage("Message contains invalid characters.");

        RuleFor(x => x.PriorMessages)
            .Must(list => list is null || list.Count <= PatientAssistantChatHistoryRules.AbsoluteMaxPriorMessages)
            .WithMessage($"At most {PatientAssistantChatHistoryRules.AbsoluteMaxPriorMessages} prior messages are allowed.");

        RuleForEach(x => x.PriorMessages)
            .ChildRules(prior =>
            {
                prior.RuleFor(p => p.Role)
                    .Must(role => PatientAssistantChatHistoryRules.TryNormalizeRole(role, out _))
                    .WithMessage("Prior message role must be user or assistant.");

                prior.RuleFor(p => p.Content)
                    .NotEmpty()
                    .MaximumLength(PatientAssistantChatHistoryRules.MaxContentLength)
                    .Must(c => c is null || !c.Contains('\0'))
                    .WithMessage("Prior message contains invalid characters.");
            })
            .When(x => x.PriorMessages is { Count: > 0 });
    }
}
