using FluentValidation;

namespace RaphCare.Application.Features.Appointments.Commands.CreateAppointment;

public class CreateAppointmentValidator : AbstractValidator<CreateAppointmentCommand>
{
    public CreateAppointmentValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.ProviderId).NotEmpty();
        RuleFor(x => x.ScheduledStart).LessThan(x => x.ScheduledEnd);
        RuleFor(x => x.Type).NotEmpty().MaximumLength(50);
    }
}

