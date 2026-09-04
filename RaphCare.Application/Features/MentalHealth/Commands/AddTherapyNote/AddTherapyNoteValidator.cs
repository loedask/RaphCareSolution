using FluentValidation;

namespace RaphCare.Application.Features.MentalHealth.Commands.AddTherapyNote;

public sealed class AddTherapyNoteValidator : AbstractValidator<AddTherapyNoteCommand>
{
    public AddTherapyNoteValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.SessionId).NotEmpty();
        RuleFor(x => x.Notes).NotEmpty().MaximumLength(8000);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
        RuleFor(x => x.CrisisRiskLevel).MaximumLength(50);
        RuleFor(x => x.CrisisDescription).MaximumLength(2000);
    }
}
