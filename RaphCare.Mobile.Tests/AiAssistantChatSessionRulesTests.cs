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
    public void NeedsGreetingSeedFalseWhenThreadHasMessages() =>
        Assert.False(AiAssistantChatSessionRules.NeedsGreetingSeed(1));
}
