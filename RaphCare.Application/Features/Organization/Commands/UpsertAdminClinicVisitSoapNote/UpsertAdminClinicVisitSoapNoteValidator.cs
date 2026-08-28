using FluentValidation;

namespace RaphCare.Application.Features.Organization.Commands.UpsertAdminClinicVisitSoapNote;

public sealed class UpsertAdminClinicVisitSoapNoteValidator : AbstractValidator<UpsertAdminClinicVisitSoapNoteCommand>
{
    public UpsertAdminClinicVisitSoapNoteValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.VisitId).NotEmpty();
        RuleFor(x => x.Subjective).MaximumLength(4000);
        RuleFor(x => x.Objective).MaximumLength(4000);
        RuleFor(x => x.Assessment).MaximumLength(4000);
        RuleFor(x => x.Plan).MaximumLength(4000);
        RuleFor(x => x)
            .Must(x =>
                !string.IsNullOrWhiteSpace(x.Subjective)
                || !string.IsNullOrWhiteSpace(x.Objective)
                || !string.IsNullOrWhiteSpace(x.Assessment)
                || !string.IsNullOrWhiteSpace(x.Plan))
            .WithMessage("Enter at least one SOAP field.");
    }
}
