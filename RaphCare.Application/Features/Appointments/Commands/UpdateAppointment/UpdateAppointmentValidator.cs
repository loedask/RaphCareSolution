using FluentValidation;

namespace RaphCare.Application.Features.Appointments.Commands.UpdateAppointment;

public class UpdateAppointmentValidator : AbstractValidator<UpdateAppointmentCommand>
{
    public UpdateAppointmentValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        When(x => x.ScheduledStart.HasValue && x.ScheduledEnd.HasValue, () =>
        {
            RuleFor(x => x.ScheduledStart)
                .LessThan(x => x.ScheduledEnd);
        });
    }
}

