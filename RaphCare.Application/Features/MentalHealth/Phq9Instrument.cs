using RaphCare.Application.Features.MentalHealth.DTOs;

namespace RaphCare.Application.Features.MentalHealth;

/// <summary>PHQ-9 instrument text, options, and severity bands used when staff record an assessment.</summary>
public static class Phq9Instrument
{
    public const string AssessmentType = "PHQ-9";
    public const int QuestionCount = 9;
    public const int MinScore = ScreeningInstrumentOptions.MinScore;
    public const int MaxItemScore = ScreeningInstrumentOptions.MaxItemScore;
    public const int MaxTotalScore = 27;

    public static readonly IReadOnlyList<string> QuestionTexts =
    [
        "Little interest or pleasure in doing things",
        "Feeling down, depressed, or hopeless",
        "Trouble falling or staying asleep, or sleeping too much",
        "Feeling tired or having little energy",
        "Poor appetite or overeating",
        "Feeling bad about yourself, or that you are a failure, or have let yourself or your family down",
        "Trouble concentrating on things, such as reading the newspaper or watching television",
        "Moving or speaking so slowly that other people could have noticed, or being so fidgety or restless that you have been moving around a lot more than usual",
        "Thoughts that you would be better off dead, or of hurting yourself in some way"
    ];

    public static MentalHealthInstrumentDto ToDto() => new()
    {
        AssessmentType = AssessmentType,
        Title = "PHQ-9",
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

    public static string OptionLabel(int score) => ScreeningInstrumentOptions.OptionLabel(score);

    public static string SeverityForTotal(int totalScore) => totalScore switch
    {
        <= 4 => "Minimal",
        <= 9 => "Mild",
        <= 14 => "Moderate",
        <= 19 => "Moderately severe",
        _ => "Severe"
    };
}
