using RaphCare.Application.Features.MentalHealth.Commands.DraftMentalHealthNote;
using RaphCare.Domain.MentalHealth;
using Xunit;

namespace RaphCare.Application.Tests.Features.MentalHealth.Commands.DraftMentalHealthNote;

public sealed class DraftMentalHealthNoteHandlerTests
{
    [Fact]
    public void BuildPromptIncludesAssessmentScoresAndMoodScoresOnlyOmitsMoodNotes()
    {
        var assessments = new List<MentalHealthAssessment>
        {
            new()
            {
                AssessmentType = "PHQ-9",
                ConductedAt = new DateTime(2026, 9, 1, 10, 0, 0, DateTimeKind.Utc),
                SeverityLevel = "Mild",
                TotalScore = 8
            }
        };
        var moods = new List<MoodLog>
        {
            new()
            {
                LoggedAt = new DateTime(2026, 9, 2, 8, 0, 0, DateTimeKind.Utc),
                MoodScore = 3,
                Notes = "SECRET free text that must not appear",
                IsFlagged = true
            }
        };

        var prompt = DraftMentalHealthNoteHandler.BuildPrompt(assessments, moods);

        Assert.Contains("PHQ-9", prompt, StringComparison.Ordinal);
        Assert.Contains("severity Mild", prompt, StringComparison.Ordinal);
        Assert.Contains("score 8", prompt, StringComparison.Ordinal);
        Assert.Contains("score 3", prompt, StringComparison.Ordinal);
        Assert.Contains("(flagged)", prompt, StringComparison.Ordinal);
        Assert.Contains("notes omitted", prompt, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SECRET", prompt, StringComparison.Ordinal);
        Assert.DoesNotContain("free text", prompt, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildPromptHandlesEmptyLists()
    {
        var prompt = DraftMentalHealthNoteHandler.BuildPrompt([], []);
        Assert.Contains("Recent assessments: none.", prompt, StringComparison.Ordinal);
        Assert.Contains("Recent mood check-ins: none.", prompt, StringComparison.Ordinal);
    }
}
