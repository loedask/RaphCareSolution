using FluentValidation;

namespace RaphCare.Application.Features.Organization.Commands.UpdateAdminFacility;

public sealed class UpdateAdminFacilityValidator : AbstractValidator<UpdateAdminFacilityCommand>
{
    public UpdateAdminFacilityValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.FacilityId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Address).MaximumLength(256);
        RuleFor(x => x.City).MaximumLength(256);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(256);
    }
}
