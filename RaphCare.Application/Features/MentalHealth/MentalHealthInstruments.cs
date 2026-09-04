using RaphCare.Application.Features.MentalHealth.DTOs;

namespace RaphCare.Application.Features.MentalHealth;

/// <summary>Resolves supported staff screening instruments (PHQ-9, GAD-7).</summary>
public static class MentalHealthInstruments
{
    public static bool TryResolve(string? assessmentType, out ResolvedInstrument instrument)
    {
        if (string.Equals(assessmentType, Phq9Instrument.AssessmentType, StringComparison.OrdinalIgnoreCase))
        {
            instrument = new ResolvedInstrument(
                Phq9Instrument.AssessmentType,
                Phq9Instrument.QuestionCount,
                Phq9Instrument.QuestionTexts,
                Phq9Instrument.SeverityForTotal,
                Phq9Instrument.ToDto);
            return true;
        }

        if (string.Equals(assessmentType, Gad7Instrument.AssessmentType, StringComparison.OrdinalIgnoreCase))
        {
            instrument = new ResolvedInstrument(
                Gad7Instrument.AssessmentType,
                Gad7Instrument.QuestionCount,
                Gad7Instrument.QuestionTexts,
                Gad7Instrument.SeverityForTotal,
                Gad7Instrument.ToDto);
            return true;
        }

        instrument = default!;
        return false;
    }

    public static MentalHealthInstrumentDto? GetDto(string? assessmentType) =>
        TryResolve(assessmentType, out var instrument) ? instrument.ToDto() : null;

    public sealed record ResolvedInstrument(
        string AssessmentType,
        int QuestionCount,
        IReadOnlyList<string> QuestionTexts,
        Func<int, string> SeverityForTotal,
        Func<MentalHealthInstrumentDto> ToDto);
}
