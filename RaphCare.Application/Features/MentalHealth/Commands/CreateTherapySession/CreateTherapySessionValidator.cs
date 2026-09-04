using FluentValidation;

namespace RaphCare.Application.Features.MentalHealth.Commands.CreateTherapySession;

public sealed class CreateTherapySessionValidator : AbstractValidator<CreateTherapySessionCommand>
{
    public CreateTherapySessionValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.SessionType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Summary).MaximumLength(4000);
    }
}
