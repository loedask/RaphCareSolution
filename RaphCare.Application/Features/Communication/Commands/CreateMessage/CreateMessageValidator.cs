using FluentValidation;

namespace RaphCare.Application.Features.Communication.Commands.CreateMessage;

public class CreateMessageValidator : AbstractValidator<CreateMessageCommand>
{
    public CreateMessageValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.RecipientUserId).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Channel).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Body).NotEmpty();
    }
}
