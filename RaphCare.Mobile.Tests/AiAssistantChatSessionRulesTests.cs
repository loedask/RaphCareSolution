using RaphCare.Mobile.Core.Common.AiAssistant;
using Xunit;

namespace RaphCare.Mobile.Tests;

public sealed class AiAssistantChatSessionRulesTests
{
    [Fact]
    public void TryNormalizeOutgoingReturnsNullWhenBlank()
    {
        Assert.Null(AiAssistantChatSessionRules.TryNormalizeOutgoing(null));
        Assert.Null(AiAssistantChatSessionRules.TryNormalizeOutgoing(""));
        Assert.Null(AiAssistantChatSessionRules.TryNormalizeOutgoing("   "));
    }

    [Fact]
    public void TryNormalizeOutgoingTrimsWhitespace() =>
        Assert.Equal("hello", AiAssistantChatSessionRules.TryNormalizeOutgoing("  hello  "));

    [Fact]
    public void TryNormalizeOutgoingCapsAtMaxLength()
    {
        var longText = new string('a', AiAssistantChatSessionRules.MaxMessageLength + 50);
        var normalized = AiAssistantChatSessionRules.TryNormalizeOutgoing(longText);
        Assert.NotNull(normalized);
        Assert.Equal(AiAssistantChatSessionRules.MaxMessageLength, normalized!.Length);
    }

    [Fact]
    public void NeedsGreetingSeedWhenEmpty() =>
        Assert.True(AiAssistantChatSessionRules.NeedsGreetingSeed(0));

    [Fact]
    public void SelectPriorForRequestKeepsLastEight()
    {
        var messages = Enumerable.Range(1, 12)
            .Select(i => (IsFromUser: i % 2 == 1, Text: $"m{i}"))
            .ToList();

        var prior = AiAssistantChatSessionRules.SelectPriorForRequest(messages);

        Assert.Equal(8, prior.Count);
        Assert.Equal("m5", prior[0].Text);
        Assert.Equal("m12", prior[^1].Text);
    }

    [Fact]
    public void SelectPriorForRequestSkipsBlankText()
    {
        var prior = AiAssistantChatSessionRules.SelectPriorForRequest(
        [
            (false, "  "),
            (true, "hello"),
            (false, "hi"),
        ]);

        Assert.Equal(2, prior.Count);
        Assert.Equal("hello", prior[0].Text);
    }
}
