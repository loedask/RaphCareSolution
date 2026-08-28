using FluentValidation;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicBed;

public sealed class CreateAdminClinicBedValidator : AbstractValidator<CreateAdminClinicBedCommand>
{
    public CreateAdminClinicBedValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.RoomId).NotEmpty();
        RuleFor(x => x.Label).NotEmpty().MaximumLength(50);
    }
}
