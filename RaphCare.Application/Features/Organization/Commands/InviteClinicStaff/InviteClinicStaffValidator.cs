using FluentValidation;

namespace RaphCare.Application.Features.Organization.Commands.InviteClinicStaff;

public sealed class InviteClinicStaffValidator : AbstractValidator<InviteClinicStaffCommand>
{
    public InviteClinicStaffValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
    }
}
