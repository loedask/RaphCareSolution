using FluentValidation;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicProviderSchedule;

public sealed class CreateAdminClinicProviderScheduleValidator : AbstractValidator<CreateAdminClinicProviderScheduleCommand>
{
    public CreateAdminClinicProviderScheduleValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.ProviderId).NotEmpty();
        RuleFor(x => x.EndTime).GreaterThan(x => x.StartTime)
            .WithMessage("End time must be after start time.");
    }
}
