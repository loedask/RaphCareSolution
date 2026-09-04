using RaphCare.Application.Features.MentalHealth.DTOs;

namespace RaphCare.Application.Features.MentalHealth;

/// <summary>GAD-7 instrument text, options, and severity bands used when staff record an assessment.</summary>
public static class Gad7Instrument
{
    public const string AssessmentType = "GAD-7";
    public const int QuestionCount = 7;
    public const int MaxTotalScore = 21;

    public static readonly IReadOnlyList<string> QuestionTexts =
    [
        "Feeling nervous, anxious, or on edge",
        "Not being able to stop or control worrying",
        "Worrying too much about different things",
        "Trouble relaxing",
        "Being so restless that it is hard to sit still",
        "Becoming easily annoyed or irritable",
        "Feeling afraid as if something awful might happen"
    ];

    public static MentalHealthInstrumentDto ToDto() => new()
    {
        AssessmentType = AssessmentType,
        Title = "GAD-7",
        Instructions = "Over the last 2 weeks, how often has the patient been bothered by each of the following?",
        Questions = QuestionTexts
            .Select((text, index) => new MentalHealthInstrumentQuestionDto
            {
                Order = index + 1,
                QuestionText = text
            })
            .ToList(),
        Options = ScreeningInstrumentOptions.ToDtoList()
    };

    public static string SeverityForTotal(int totalScore) => totalScore switch
    {
        <= 4 => "Minimal",
        <= 9 => "Mild",
        <= 14 => "Moderate",
        _ => "Severe"
    };
}
