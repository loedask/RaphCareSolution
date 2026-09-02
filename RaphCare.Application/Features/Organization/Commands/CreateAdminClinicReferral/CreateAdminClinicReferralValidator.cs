using FluentValidation;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicReferral;

public sealed class CreateAdminClinicReferralValidator : AbstractValidator<CreateAdminClinicReferralCommand>
{
    public CreateAdminClinicReferralValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.ReferredTo).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Reason).MaximumLength(500);
        RuleFor(x => x.Specialty).MaximumLength(100);
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}
