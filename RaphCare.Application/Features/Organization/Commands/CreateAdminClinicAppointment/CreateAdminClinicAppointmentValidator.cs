using FluentValidation;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicAppointment;

public sealed class CreateAdminClinicAppointmentValidator : AbstractValidator<CreateAdminClinicAppointmentCommand>
{
    public CreateAdminClinicAppointmentValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.ProviderId).NotEmpty();
        RuleFor(x => x.Type).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Reason).MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.Reason));
        RuleFor(x => x.ScheduledEnd).GreaterThan(x => x.ScheduledStart)
            .WithMessage("End time must be after start time.");
    }
}
