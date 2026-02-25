using FluentValidation;

namespace RaphCare.Application.Features.Clinical.Commands.CreateVisit;

public class CreateVisitValidator : AbstractValidator<CreateVisitCommand>
{
    public CreateVisitValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.AppointmentId).NotEmpty();
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.ProviderId).NotEmpty();
        RuleFor(x => x.VisitStart).LessThanOrEqualTo(DateTime.UtcNow.AddMinutes(5));
        RuleFor(x => x.VisitType).NotEmpty().MaximumLength(100);
    }
}

