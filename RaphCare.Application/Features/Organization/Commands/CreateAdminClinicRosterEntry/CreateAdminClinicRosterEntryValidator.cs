using FluentValidation;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicRosterEntry;

public sealed class CreateAdminClinicRosterEntryValidator : AbstractValidator<CreateAdminClinicRosterEntryCommand>
{
    public CreateAdminClinicRosterEntryValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.ApplicationUserId).NotEmpty();
        RuleFor(x => x.DutyDate).NotEmpty();
        RuleFor(x => x.ShiftLabel)
            .NotEmpty()
            .Must(label => ClinicRosterShift.Allowed.Contains(label))
            .WithMessage("Shift must be Morning, Afternoon, or Night.");
        RuleFor(x => x.Note).MaximumLength(200).When(x => !string.IsNullOrWhiteSpace(x.Note));
    }
}
