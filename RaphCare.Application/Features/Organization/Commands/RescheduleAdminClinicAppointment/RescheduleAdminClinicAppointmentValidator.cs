using FluentValidation;

namespace RaphCare.Application.Features.Organization.Commands.RescheduleAdminClinicAppointment;

public sealed class RescheduleAdminClinicAppointmentValidator : AbstractValidator<RescheduleAdminClinicAppointmentCommand>
{
    public RescheduleAdminClinicAppointmentValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.AppointmentId).NotEmpty();
        RuleFor(x => x.Reason).MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.Reason));
        RuleFor(x => x.ScheduledEnd).GreaterThan(x => x.ScheduledStart)
            .WithMessage("End time must be after start time.");
    }
}
