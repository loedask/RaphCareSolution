using Xunit;

namespace RaphCare.Mobile.Tests.Core.Common.AiAssistant;

/// <summary>
/// Mobile.Tests cannot host MAUI ViewModels. These source checks lock the Send crash fixes.
/// </summary>
public sealed class AiAssistantSendWiringTests
{
    [Fact]
    public void SendAsyncMustCatchExceptionsSoAndroidDoesNotForceClose()
    {
        var text = ReadRepoFile(
            Path.Combine(
                "RaphCare.Mobile",
                "Core",
                "Features",
                "AiAssistant",
                "ViewModels",
                "AiAssistantViewModel.cs"));

        var sendIdx = text.IndexOf("private async Task SendAsync()", StringComparison.Ordinal);
        Assert.True(sendIdx >= 0);
        var body = text[sendIdx..];
        Assert.Contains("catch (Exception)", body, StringComparison.Ordinal);
        Assert.Contains("AiAssistantSendFailed", body, StringComparison.Ordinal);
    }

    [Fact]
    public void ChatPageMustGuardScrollToAfterSend()
    {
        var text = ReadRepoFile(
            Path.Combine(
                "RaphCare.Mobile",
                "Core",
                "Features",
                "AiAssistant",
                "Views",
                "AiAssistantPage.xaml.cs"));

        Assert.Contains("ScrollTo(index", text, StringComparison.Ordinal);
        Assert.Contains("catch (Exception", text, StringComparison.Ordinal);
        Assert.Contains("Task.Delay(50)", text, StringComparison.Ordinal);
    }

    private static string ReadRepoFile(string relativePath)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, relativePath);
            if (File.Exists(candidate))
                return File.ReadAllText(candidate);

            if (File.Exists(Path.Combine(dir.FullName, "RaphCareSolution.slnx")))
            {
                candidate = Path.Combine(dir.FullName, relativePath);
                if (File.Exists(candidate))
                    return File.ReadAllText(candidate);
            }

            dir = dir.Parent;
        }

        throw new FileNotFoundException($"Could not locate {relativePath}.");
    }
}
