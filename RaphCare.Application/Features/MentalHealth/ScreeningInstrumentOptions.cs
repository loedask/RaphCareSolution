using RaphCare.Application.Features.MentalHealth.DTOs;

namespace RaphCare.Application.Features.MentalHealth;

/// <summary>Shared 0–3 frequency options used by PHQ-9 and GAD-7.</summary>
public static class ScreeningInstrumentOptions
{
    public const int MinScore = 0;
    public const int MaxItemScore = 3;

    public static readonly IReadOnlyList<(int Score, string Label)> Options =
    [
        (0, "Not at all"),
        (1, "Several days"),
        (2, "More than half the days"),
        (3, "Nearly every day")
    ];

    public static IReadOnlyList<MentalHealthInstrumentOptionDto> ToDtoList() =>
        Options
            .Select(o => new MentalHealthInstrumentOptionDto
            {
                NumericScore = o.Score,
                Label = o.Label
            })
            .ToList();

    public static string OptionLabel(int score) =>
        Options.FirstOrDefault(o => o.Score == score).Label
        ?? throw new ArgumentOutOfRangeException(nameof(score), score, "Item score must be 0 to 3.");
}
