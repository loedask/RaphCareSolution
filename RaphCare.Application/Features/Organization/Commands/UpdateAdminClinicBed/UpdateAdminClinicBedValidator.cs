using FluentValidation;

namespace RaphCare.Application.Features.Organization.Commands.UpdateAdminClinicBed;

public sealed class UpdateAdminClinicBedValidator : AbstractValidator<UpdateAdminClinicBedCommand>
{
    public UpdateAdminClinicBedValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.BedId).NotEmpty();
        RuleFor(x => x.Label).NotEmpty().MaximumLength(50);
    }
}
