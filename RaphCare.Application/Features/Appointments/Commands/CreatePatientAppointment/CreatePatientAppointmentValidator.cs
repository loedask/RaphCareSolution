using FluentValidation;

namespace RaphCare.Application.Features.Appointments.Commands.CreatePatientAppointment;

public class CreatePatientAppointmentValidator : AbstractValidator<CreatePatientAppointmentCommand>
{
    public CreatePatientAppointmentValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.ProviderId).NotEmpty();
        RuleFor(x => x.ScheduledStart).LessThan(x => x.ScheduledEnd);
        RuleFor(x => x.Type).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}
