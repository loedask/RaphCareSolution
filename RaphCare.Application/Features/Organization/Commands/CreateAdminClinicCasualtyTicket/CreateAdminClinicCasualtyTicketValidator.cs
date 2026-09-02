using FluentValidation;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicCasualtyTicket;

public sealed class CreateAdminClinicCasualtyTicketValidator : AbstractValidator<CreateAdminClinicCasualtyTicketCommand>
{
    private static readonly HashSet<string> AllowedLevels =
        new(StringComparer.OrdinalIgnoreCase) { "Red", "Orange", "Yellow", "Green" };

    public CreateAdminClinicCasualtyTicketValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.TriageLevel)
            .NotEmpty()
            .Must(level => AllowedLevels.Contains(level.Trim()))
            .WithMessage("Triage level must be Red, Orange, Yellow, or Green.");
        RuleFor(x => x.ChiefComplaint).MaximumLength(500);
    }
}
