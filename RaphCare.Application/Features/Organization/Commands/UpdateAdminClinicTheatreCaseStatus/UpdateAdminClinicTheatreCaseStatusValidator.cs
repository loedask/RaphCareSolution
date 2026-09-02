using FluentValidation;

namespace RaphCare.Application.Features.Organization.Commands.UpdateAdminClinicTheatreCaseStatus;

public sealed class UpdateAdminClinicTheatreCaseStatusValidator
    : AbstractValidator<UpdateAdminClinicTheatreCaseStatusCommand>
{
    private static readonly HashSet<string> Allowed =
        new(StringComparer.OrdinalIgnoreCase) { "Scheduled", "InProgress", "Completed", "Cancelled" };

    public UpdateAdminClinicTheatreCaseStatusValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.CaseId).NotEmpty();
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(status => Allowed.Contains(status.Trim()))
            .WithMessage("Status must be Scheduled, InProgress, Completed, or Cancelled.");
    }
}
