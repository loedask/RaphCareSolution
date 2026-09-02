using FluentValidation;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicTheatreCase;

public sealed class CreateAdminClinicTheatreCaseValidator : AbstractValidator<CreateAdminClinicTheatreCaseCommand>
{
    public CreateAdminClinicTheatreCaseValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.ProcedureName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TheatreName).MaximumLength(100);
        RuleFor(x => x.SurgeonName).MaximumLength(200);
        RuleFor(x => x.Notes).MaximumLength(1000);
        RuleFor(x => x.ScheduledStart).NotEmpty();
        RuleFor(x => x)
            .Must(x => x.ScheduledEnd is null || x.ScheduledEnd >= x.ScheduledStart)
            .WithMessage("Scheduled end must be after the start time.");
    }
}
