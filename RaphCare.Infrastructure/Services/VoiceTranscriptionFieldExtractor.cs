using System.Globalization;
using System.Text.RegularExpressions;

namespace RaphCare.Infrastructure.Services;

/// <summary>Heuristic extraction of patient fields from a voice onboarding transcript.</summary>
internal static partial class VoiceTranscriptionFieldExtractor
{
    public static Dictionary<string, string> Extract(string fullText)
    {
        var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(fullText))
            return fields;

        var text = fullText.Trim();

        var name = TryExtractName(text);
        if (name is { } n)
        {
            fields["FirstName"] = n.FirstName;
            fields["LastName"] = n.LastName;
        }

        var dob = TryExtractDateOfBirth(text);
        if (dob is not null)
            fields["DateOfBirth"] = dob.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        return fields;
    }

    private static (string FirstName, string LastName)? TryExtractName(string text)
    {
        foreach (var pattern in NamePatterns())
        {
            var match = pattern.Match(text);
            if (!match.Success)
                continue;

            var raw = match.Groups["name"].Value.Trim();
            var parts = raw.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                continue;

            if (parts.Length == 1)
                return (parts[0], "Unknown");

            return (parts[0], string.Join(' ', parts.Skip(1)));
        }

        return null;
    }

    private static DateTime? TryExtractDateOfBirth(string text)
    {
        var bornMatch = BornOnPattern().Match(text);
        if (bornMatch.Success && DateTime.TryParse(bornMatch.Groups["date"].Value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var born))
            return DateTime.SpecifyKind(born.Date, DateTimeKind.Utc);

        var isoMatch = IsoDatePattern().Match(text);
        if (isoMatch.Success && DateTime.TryParse(isoMatch.Value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var iso))
            return DateTime.SpecifyKind(iso.Date, DateTimeKind.Utc);

        return null;
    }

    [GeneratedRegex(@"(?i)\b(?:my name is|i am|i'm|this is)\s+(?<name>[A-Za-z][A-Za-z'\-]*(?:\s+[A-Za-z][A-Za-z'\-]*){0,3})")]
    private static partial Regex NamePattern1();

    [GeneratedRegex(@"(?i)\bname\s+(?:is\s+)?(?<name>[A-Za-z][A-Za-z'\-]*(?:\s+[A-Za-z][A-Za-z'\-]*){0,3})")]
    private static partial Regex NamePattern2();

    private static IEnumerable<Regex> NamePatterns()
    {
        yield return NamePattern1();
        yield return NamePattern2();
    }

    [GeneratedRegex(@"(?i)\b(?:born on|date of birth is|dob is|i was born on)\s+(?<date>[\d]{1,2}\s+[A-Za-z]+\s+[\d]{4}|[A-Za-z]+\s+[\d]{1,2},?\s+[\d]{4}|[\d]{4}-[\d]{2}-[\d]{2})")]
    private static partial Regex BornOnPattern();

    [GeneratedRegex(@"\b(?<date>\d{4}-\d{2}-\d{2})\b")]
    private static partial Regex IsoDatePattern();
}
