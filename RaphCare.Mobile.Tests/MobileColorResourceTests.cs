using Xunit;

namespace RaphCare.Mobile.Tests;

/// <summary>
/// Missing StaticResource colors in Colors.xaml crash page inflate on Android
/// (Home bell → NotificationsPage closed the app when CalmColor was absent).
/// </summary>
public sealed class MobileColorResourceTests
{
    [Theory]
    [InlineData("CalmColor")]
    [InlineData("CalmAccentColor")]
    [InlineData("CalmSoftColor")]
    public void ColorsXamlDefinesCalmTokensRequiredByNotificationsAndMentalHealth(string key)
    {
        var path = FindColorsXaml();
        Assert.True(File.Exists(path), $"Expected Colors.xaml at '{path}'.");

        var xml = File.ReadAllText(path);
        Assert.Contains(
            $"x:Key=\"{key}\"",
            xml,
            StringComparison.Ordinal);
    }

    private static string FindColorsXaml()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "RaphCare.Mobile", "Resources", "Styles", "Colors.xaml");
            if (File.Exists(candidate))
                return candidate;
            dir = dir.Parent;
        }

        return Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..",
            "RaphCare.Mobile", "Resources", "Styles", "Colors.xaml"));
    }
}
