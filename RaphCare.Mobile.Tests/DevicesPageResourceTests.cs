using System.Text.RegularExpressions;
using Xunit;

namespace RaphCare.Mobile.Tests;

/// <summary>
/// Missing StaticResource styles on DevicesPage crash Shell inflate on Android.
/// Home "See all" for Health Summary and Connected Devices navigates there; SafeShellNavigator
/// swallows the failure so the tap looks dead.
/// </summary>
public sealed class DevicesPageResourceTests
{
    private static readonly Regex StyleResource = new(
        @"Style=""\{StaticResource (?<key>[^}]+)\}""",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    [Fact]
    public void DevicesPageSerialEntryUsesDefaultEntryNotMissingTextEntry()
    {
        var pageXml = File.ReadAllText(FindRepoFile(
            Path.Combine("RaphCare.Mobile", "Core", "Features", "Devices", "Views", "DevicesPage.xaml")));

        Assert.DoesNotContain(
            "StaticResource TextEntry",
            pageXml,
            StringComparison.Ordinal);
        Assert.Contains(
            "StaticResource DefaultEntry",
            pageXml,
            StringComparison.Ordinal);
    }

    [Fact]
    public void DevicesPageStyleResourcesExistInMobileStyles()
    {
        var pageXml = File.ReadAllText(FindRepoFile(
            Path.Combine("RaphCare.Mobile", "Core", "Features", "Devices", "Views", "DevicesPage.xaml")));
        var stylesDir = FindRepoFile(Path.Combine("RaphCare.Mobile", "Resources", "Styles"));
        var stylesXml = string.Join(
            '\n',
            Directory.EnumerateFiles(stylesDir, "*.xaml").Select(File.ReadAllText));

        var keys = StyleResource.Matches(pageXml)
            .Select(m => m.Groups["key"].Value.Trim())
            .Distinct(StringComparer.Ordinal)
            .OrderBy(k => k, StringComparer.Ordinal)
            .ToList();

        Assert.NotEmpty(keys);
        foreach (var key in keys)
        {
            Assert.Contains(
                $"x:Key=\"{key}\"",
                stylesXml,
                StringComparison.Ordinal);
        }
    }

    private static string FindRepoFile(string relativePath)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, relativePath);
            if (File.Exists(candidate) || Directory.Exists(candidate))
                return candidate;
            dir = dir.Parent;
        }

        return Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..",
            relativePath));
    }
}
