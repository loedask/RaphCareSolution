using FluentValidation;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicProvider;

public sealed class CreateAdminClinicProviderValidator : AbstractValidator<CreateAdminClinicProviderCommand>
{
    public CreateAdminClinicProviderValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.LicenseNumber).MaximumLength(256).When(x => !string.IsNullOrWhiteSpace(x.LicenseNumber));
    }
}
