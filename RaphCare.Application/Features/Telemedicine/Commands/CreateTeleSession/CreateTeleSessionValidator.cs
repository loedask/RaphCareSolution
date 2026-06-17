using FluentValidation;

namespace RaphCare.Application.Features.Telemedicine.Commands.CreateTeleSession;

public class CreateTeleSessionValidator : AbstractValidator<CreateTeleSessionCommand>
{
    public CreateTeleSessionValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.AppointmentId).NotEmpty();
        RuleFor(x => x.VisitId).NotEmpty();
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.ProviderId).NotEmpty();
        RuleFor(x => x.Platform).NotEmpty().MaximumLength(100);
    }
}

