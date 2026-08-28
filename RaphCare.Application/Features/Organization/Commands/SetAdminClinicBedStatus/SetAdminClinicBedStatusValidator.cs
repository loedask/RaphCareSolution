using FluentValidation;

namespace RaphCare.Application.Features.Organization.Commands.SetAdminClinicBedStatus;

public sealed class SetAdminClinicBedStatusValidator : AbstractValidator<SetAdminClinicBedStatusCommand>
{
    private static readonly string[] Allowed = ["Available", "Maintenance"];

    public SetAdminClinicBedStatusValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.BedId).NotEmpty();
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(s => Allowed.Contains(s.Trim(), StringComparer.OrdinalIgnoreCase))
            .WithMessage("Status must be Available or Maintenance.");
    }
}
