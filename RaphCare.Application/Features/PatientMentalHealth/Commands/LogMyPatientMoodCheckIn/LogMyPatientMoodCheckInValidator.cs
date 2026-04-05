using FluentValidation;

namespace RaphCare.Application.Features.PatientMentalHealth.Commands.LogMyPatientMoodCheckIn;

public sealed class LogMyPatientMoodCheckInValidator : AbstractValidator<LogMyPatientMoodCheckInCommand>
{
    public LogMyPatientMoodCheckInValidator()
    {
        RuleFor(x => x.MoodScore).InclusiveBetween(0, 3);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}
