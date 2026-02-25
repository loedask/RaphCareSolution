using FluentValidation;

namespace RaphCare.Application.Features.Communication.Commands.UpdateMessage;

public class UpdateMessageValidator : AbstractValidator<UpdateMessageCommand>
{
    public UpdateMessageValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
