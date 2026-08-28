using FluentValidation;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminFacility;

public sealed class CreateAdminFacilityValidator : AbstractValidator<CreateAdminFacilityCommand>
{
    public CreateAdminFacilityValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Address).MaximumLength(256);
        RuleFor(x => x.City).MaximumLength(256);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(256);
    }
}
