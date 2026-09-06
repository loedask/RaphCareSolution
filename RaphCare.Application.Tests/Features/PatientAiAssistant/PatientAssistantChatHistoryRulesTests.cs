using RaphCare.Application.Features.PatientAiAssistant;
using RaphCare.Application.Features.PatientAiAssistant.DTOs;
using Xunit;

namespace RaphCare.Application.Tests.Features.PatientAiAssistant;

public sealed class PatientAssistantChatHistoryRulesTests
{
    [Fact]
    public void NormalizeAndWindowReturnsEmptyWhenNullOrZeroLimit()
    {
        Assert.Empty(PatientAssistantChatHistoryRules.NormalizeAndWindow(null, 8));
        Assert.Empty(PatientAssistantChatHistoryRules.NormalizeAndWindow(
            [new PatientAssistantPriorMessageDto { Role = "user", Content = "hi" }],
            0));
    }

    [Fact]
    public void NormalizeAndWindowDropsInvalidRolesAndEmptyContent()
    {
        var prior = new List<PatientAssistantPriorMessageDto>
        {
            new() { Role = "system", Content = "ignore" },
            new() { Role = "user", Content = "  " },
            new() { Role = "USER", Content = "hello" },
            new() { Role = "assistant", Content = "hi there" },
        };

        var result = PatientAssistantChatHistoryRules.NormalizeAndWindow(prior, 8);

        Assert.Equal(2, result.Count);
        Assert.Equal(("user", "hello"), result[0]);
        Assert.Equal(("assistant", "hi there"), result[1]);
    }

    [Fact]
    public void NormalizeAndWindowKeepsOnlyLastN()
    {
        var prior = Enumerable.Range(1, 10)
            .Select(i => new PatientAssistantPriorMessageDto
            {
                Role = i % 2 == 0 ? "assistant" : "user",
                Content = $"m{i}",
            })
            .ToList();

        var result = PatientAssistantChatHistoryRules.NormalizeAndWindow(prior, 8);

        Assert.Equal(8, result.Count);
        Assert.Equal("m3", result[0].Content);
        Assert.Equal("m10", result[^1].Content);
    }

    [Fact]
    public void NormalizeAndWindowCapsAbsoluteMaxEvenIfOptionIsHigher()
    {
        var prior = Enumerable.Range(1, 20)
            .Select(i => new PatientAssistantPriorMessageDto { Role = "user", Content = $"m{i}" })
            .ToList();

        var result = PatientAssistantChatHistoryRules.NormalizeAndWindow(prior, 100);

        Assert.Equal(PatientAssistantChatHistoryRules.AbsoluteMaxPriorMessages, result.Count);
        Assert.Equal("m9", result[0].Content);
    }
}
