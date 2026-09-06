using RaphCare.Mobile.Core.Common.Home;
using Xunit;

namespace RaphCare.Mobile.Tests;

public sealed class HomeUpcomingAppointmentRulesTests
{
    private sealed record Sample(DateTime Start, DateTime End, string Status);

    [Fact]
    public void PickNextUpcomingSkipsPastAndCancelled()
    {
        var now = new DateTime(2026, 9, 5, 12, 0, 0, DateTimeKind.Utc);
        var items = new[]
        {
            new Sample(now.AddHours(-3), now.AddHours(-2), "Completed"),
            new Sample(now.AddHours(1), now.AddHours(2), "Cancelled"),
            new Sample(now.AddHours(3), now.AddHours(4), "Scheduled"),
            new Sample(now.AddHours(5), now.AddHours(6), "Scheduled"),
        };

        var next = HomeUpcomingAppointmentRules.PickNextUpcoming(
            items,
            now,
            a => a.Start,
            a => a.End,
            a => a.Status);

        Assert.NotNull(next);
        Assert.Equal(now.AddHours(3), next!.Start);
    }

    [Theory]
    [InlineData("Video", true)]
    [InlineData("Telehealth", true)]
    [InlineData("InPerson", false)]
    [InlineData(null, false)]
    public void LooksLikeVideoDetectsTeleTypes(string? type, bool expected) =>
        Assert.Equal(expected, HomeUpcomingAppointmentRules.LooksLikeVideo(type));

    [Fact]
    public void InitialsFromLabelUsesTwoWords() =>
        Assert.Equal("GC", HomeUpcomingAppointmentRules.InitialsFromLabel("General Consultation"));
}
