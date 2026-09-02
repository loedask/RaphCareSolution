using FluentValidation;

namespace RaphCare.Application.Features.Organization.Commands.UpdateAdminClinicReferralStatus;

public sealed class UpdateAdminClinicReferralStatusValidator
    : AbstractValidator<UpdateAdminClinicReferralStatusCommand>
{
    private static readonly HashSet<string> Allowed =
        new(StringComparer.OrdinalIgnoreCase) { "Sent", "Accepted", "Completed", "Cancelled" };

    public UpdateAdminClinicReferralStatusValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.ReferralId).NotEmpty();
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(status => Allowed.Contains(status.Trim()))
            .WithMessage("Status must be Sent, Accepted, Completed, or Cancelled.");
    }
}
