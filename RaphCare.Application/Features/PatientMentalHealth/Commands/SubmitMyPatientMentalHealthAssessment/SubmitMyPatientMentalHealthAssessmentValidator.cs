using FluentValidation;
using RaphCare.Application.Features.MentalHealth;

namespace RaphCare.Application.Features.PatientMentalHealth.Commands.SubmitMyPatientMentalHealthAssessment;

public sealed class SubmitMyPatientMentalHealthAssessmentValidator
    : AbstractValidator<SubmitMyPatientMentalHealthAssessmentCommand>
{
    public SubmitMyPatientMentalHealthAssessmentValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.AssessmentType)
            .NotEmpty()
            .Must(t => MentalHealthInstruments.TryResolve(t, out _))
            .WithMessage($"Only {Phq9Instrument.AssessmentType} or {Gad7Instrument.AssessmentType} is supported.");
        RuleFor(x => x.Answers)
            .NotNull()
            .Must((cmd, answers) =>
                MentalHealthInstruments.TryResolve(cmd.AssessmentType, out var instrument)
                && answers.Count == instrument.QuestionCount)
            .WithMessage(cmd =>
                MentalHealthInstruments.TryResolve(cmd.AssessmentType, out var instrument)
                    ? $"{instrument.AssessmentType} requires exactly {instrument.QuestionCount} answers."
                    : "Unsupported assessment type.");
        RuleFor(x => x.Answers)
            .Must((cmd, answers) =>
            {
                if (!MentalHealthInstruments.TryResolve(cmd.AssessmentType, out var instrument))
                    return false;
                if (answers.Count != instrument.QuestionCount)
                    return false;
                var orders = answers.Select(a => a.Order).OrderBy(o => o).ToArray();
                for (var i = 0; i < instrument.QuestionCount; i++)
                {
                    if (orders[i] != i + 1)
                        return false;
                }

                return true;
            });
        RuleForEach(x => x.Answers).ChildRules(answer =>
        {
            answer.RuleFor(a => a.NumericScore)
                .InclusiveBetween(ScreeningInstrumentOptions.MinScore, ScreeningInstrumentOptions.MaxItemScore);
        });
    }
}
