using FluentValidation;
using RaphCare.Domain.Identity;

namespace RaphCare.Application.Features.Organization.Commands.InviteClinicStaff;

public sealed class InviteClinicStaffValidator : AbstractValidator<InviteClinicStaffCommand>
{
    public InviteClinicStaffValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.JobRole)
            .Must(role => RaphCareRoles.JobRoles.Contains(RaphCareRoles.NormalizeJobRole(role)))
            .WithMessage("Choose a staff job: staff, doctor, pharmacist, or lab technician.");
    }
}
