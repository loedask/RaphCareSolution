using FluentValidation;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicVisitNote;

public sealed class CreateAdminClinicVisitNoteValidator : AbstractValidator<CreateAdminClinicVisitNoteCommand>
{
    public CreateAdminClinicVisitNoteValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.VisitId).NotEmpty();
        RuleFor(x => x.Notes).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Category).MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.Category));
    }
}
