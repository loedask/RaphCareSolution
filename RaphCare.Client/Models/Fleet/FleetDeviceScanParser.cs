using System.Text;
using System.Text.RegularExpressions;

namespace RaphCare.Client.Models.Fleet;

/// <summary>Parses packaging barcodes or Device Info OCR text into fleet stock-in fields.</summary>
public static partial class FleetDeviceScanParser
{
    private static readonly string[] KnownModels = ["E585", "E580", "Y6 Pro"];

    /// <summary>Result of parsing scan or OCR text for the Add to stock form.</summary>
    public sealed record Result(
        string? SuggestedModel,
        string? PreferredValue,
        bool PreferredLooksLikeMac,
        IReadOnlyList<Candidate> Candidates);

    /// <summary>A fillable value found in scan or OCR output.</summary>
    public sealed record Candidate(string Value, string Kind);

    /// <summary>Suggested serial and MAC to put on the Add to stock form (both may be set).</summary>
    public sealed record FillSuggestion(string? Serial, string? Mac);

    /// <summary>Parses free text from OCR or a barcode payload.</summary>
    public static Result Parse(string? rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText))
            return new Result(null, null, false, Array.Empty<Candidate>());

        var text = rawText.Trim();
        var candidates = new List<Candidate>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        void Add(string value, string kind)
        {
            var trimmed = value.Trim();
            if (trimmed.Length is < 4 or > 100)
                return;
            if (!seen.Add(trimmed))
                return;
            candidates.Add(new Candidate(trimmed, kind));
        }

        foreach (Match match in MacSeparatedRegex().Matches(text))
        {
            var normalized = NormalizeMac(match.Value);
            if (normalized is not null)
                Add(normalized, "Mac");
        }

        foreach (Match match in MacCompactRegex().Matches(text))
        {
            var normalized = NormalizeMac(match.Value);
            if (normalized is not null)
                Add(normalized, "Mac");
        }

        foreach (Match match in SerialLabelRegex().Matches(text))
        {
            var labeled = match.Groups[1].Value.Trim();
            if (LooksLikeMac(labeled))
            {
                var normalized = NormalizeMac(labeled);
                if (normalized is not null)
                    Add(normalized, "Mac");
                continue;
            }

            Add(labeled, "Serial");
        }

        // Packaging-style tokens (RC-E585-1001). Skip firmware, MAC, model labels, and Device Info chrome.
        foreach (Match match in SerialTokenRegex().Matches(text))
        {
            var token = match.Value.Trim();
            if (LooksLikeFirmwareVersion(token) || LooksLikeMac(token))
                continue;
            if (IsModelToken(token) || IsNoiseWord(token) || !LooksLikePackagingSerial(token))
                continue;
            Add(token, "Serial");
        }

        var suggestedModel = DetectModel(text);
        var preferred = PreferValue(candidates);
        var looksLikeMac = preferred is not null && LooksLikeMac(preferred);

        return new Result(suggestedModel, preferred, looksLikeMac, candidates);
    }

    /// <summary>Parses a live barcode or QR payload into a stock serial candidate.</summary>
    public static Result ParseBarcode(string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
            return new Result(null, null, false, Array.Empty<Candidate>());

        var value = rawValue.Trim();

        // QR may wrap a serial in a URL query or path segment.
        if (Uri.TryCreate(value, UriKind.Absolute, out var uri))
        {
            var fromQuery = ExtractSerialFromQuery(uri.Query);
            if (!string.IsNullOrWhiteSpace(fromQuery))
                return Parse(fromQuery);

            var lastSegment = uri.AbsolutePath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries)
                .LastOrDefault();
            if (!string.IsNullOrWhiteSpace(lastSegment) && lastSegment.Length is >= 4 and <= 100)
                return Parse(Uri.UnescapeDataString(lastSegment));
        }

        var parsed = Parse(value);
        if (!string.IsNullOrWhiteSpace(parsed.PreferredValue))
            return parsed;

        // Many packaging barcodes are the serial itself (including numeric-only codes).
        if (value.Length is >= 4 and <= 100)
        {
            if (LooksLikeMac(value))
            {
                var mac = NormalizeMac(value)!;
                return new Result(DetectModel(value), mac, true, [new Candidate(mac, "Mac")]);
            }

            return new Result(
                DetectModel(value),
                value,
                false,
                [new Candidate(value, "Serial")]);
        }

        return parsed;
    }

    /// <summary>
    /// Picks serial and MAC for the form. Packaging serial wins for Serial; any MAC candidate fills Mac.
    /// </summary>
    public static FillSuggestion SuggestFill(Result result)
    {
        string? serial = null;
        string? mac = null;

        foreach (var candidate in result.Candidates)
        {
            if (string.Equals(candidate.Kind, "Serial", StringComparison.OrdinalIgnoreCase))
                serial ??= candidate.Value;
            else if (string.Equals(candidate.Kind, "Mac", StringComparison.OrdinalIgnoreCase))
                mac ??= candidate.Value;
        }

        if (serial is null
            && !string.IsNullOrWhiteSpace(result.PreferredValue)
            && !result.PreferredLooksLikeMac)
        {
            serial = result.PreferredValue;
        }

        if (mac is null
            && !string.IsNullOrWhiteSpace(result.PreferredValue)
            && result.PreferredLooksLikeMac)
        {
            mac = result.PreferredValue;
        }

        return new FillSuggestion(serial, mac);
    }

    private static string? PreferValue(IReadOnlyList<Candidate> candidates)
    {
        var serial = candidates.FirstOrDefault(c =>
            string.Equals(c.Kind, "Serial", StringComparison.OrdinalIgnoreCase));
        if (serial is not null)
            return serial.Value;

        var mac = candidates.FirstOrDefault(c =>
            string.Equals(c.Kind, "Mac", StringComparison.OrdinalIgnoreCase));
        return mac?.Value;
    }

    private static string? DetectModel(string text)
    {
        if (Y6Regex().IsMatch(text))
            return "Y6 Pro";

        // ET585 / E585 before ET580 / E580 so longer OEM labels win when both appear (unlikely).
        if (E585Regex().IsMatch(text))
            return "E585";

        if (E580Regex().IsMatch(text))
            return "E580";

        foreach (var model in KnownModels)
        {
            if (text.Contains(model, StringComparison.OrdinalIgnoreCase))
                return model;
        }

        return null;
    }

    private static string? ExtractSerialFromQuery(string query)
    {
        if (string.IsNullOrEmpty(query))
            return null;

        var pairs = query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries);
        foreach (var pair in pairs)
        {
            var parts = pair.Split('=', 2);
            if (parts.Length != 2)
                continue;
            var key = Uri.UnescapeDataString(parts[0]);
            if (!key.Equals("serial", StringComparison.OrdinalIgnoreCase)
                && !key.Equals("serialNumber", StringComparison.OrdinalIgnoreCase)
                && !key.Equals("sn", StringComparison.OrdinalIgnoreCase))
                continue;
            return Uri.UnescapeDataString(parts[1]).Trim();
        }

        return null;
    }

    /// <summary>Canonical <c>AA:BB:CC:DD:EE:FF</c>, or null when not a 6-byte hex MAC.</summary>
    private static string? NormalizeMac(string mac)
    {
        var hex = HexOnlyRegex().Replace(mac.Trim(), string.Empty);
        if (hex.Length != 12 || !HexOnlyValidRegex().IsMatch(hex))
            return null;

        var sb = new StringBuilder(17);
        for (var i = 0; i < 12; i += 2)
        {
            if (i > 0)
                sb.Append(':');
            sb.Append(char.ToUpperInvariant(hex[i]));
            sb.Append(char.ToUpperInvariant(hex[i + 1]));
        }

        return sb.ToString();
    }

    private static bool LooksLikeMac(string value)
    {
        var trimmed = value.Trim();
        if (MacSeparatedRegex().IsMatch(trimmed))
            return true;

        // OCR often drops colons (6F9ACBACE445). Require a hex letter so all-digit TP blobs stay out.
        return MacCompactRegex().IsMatch(trimmed);
    }

    private static bool LooksLikeFirmwareVersion(string value) =>
        FirmwareVersionRegex().IsMatch(value);

    private static bool IsModelToken(string token)
    {
        var t = token.Trim();
        return t.Equals("E585", StringComparison.OrdinalIgnoreCase)
               || t.Equals("E580", StringComparison.OrdinalIgnoreCase)
               || t.Equals("ET585", StringComparison.OrdinalIgnoreCase)
               || t.Equals("ET580", StringComparison.OrdinalIgnoreCase)
               || t.Equals("Y6", StringComparison.OrdinalIgnoreCase)
               || t.Equals("Y6Pro", StringComparison.OrdinalIgnoreCase)
               || t.Equals("Y6 Pro", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsNoiseWord(string token) =>
        token.Equals("Device", StringComparison.OrdinalIgnoreCase)
        || token.Equals("Info", StringComparison.OrdinalIgnoreCase)
        || token.Equals("Version", StringComparison.OrdinalIgnoreCase)
        || token.Equals("MAC", StringComparison.OrdinalIgnoreCase)
        || token.Equals("TP", StringComparison.OrdinalIgnoreCase)
        || token.Equals("Serial", StringComparison.OrdinalIgnoreCase)
        || token.Equals("Number", StringComparison.OrdinalIgnoreCase)
        || token.Equals("Model", StringComparison.OrdinalIgnoreCase)
        || token.Equals("Watch", StringComparison.OrdinalIgnoreCase)
        || token.Equals("Band", StringComparison.OrdinalIgnoreCase);

    /// <summary>Packaging serials include a letter and a digit (RC-E585-1001). Numeric-only codes use ParseBarcode.</summary>
    private static bool LooksLikePackagingSerial(string token)
    {
        var hasDigit = false;
        var hasLetter = false;
        foreach (var c in token)
        {
            if (char.IsDigit(c))
                hasDigit = true;
            else if (char.IsLetter(c))
                hasLetter = true;
        }

        return hasDigit && hasLetter;
    }

    [GeneratedRegex(@"\b(?:[0-9A-Fa-f]{2}[:\-]){5}[0-9A-Fa-f]{2}\b", RegexOptions.CultureInvariant)]
    private static partial Regex MacSeparatedRegex();

    /// <summary>Compact 12-hex MAC from OCR without separators. Requires A-F so digit-only TP blobs are skipped.</summary>
    [GeneratedRegex(@"\b(?=[0-9A-Fa-f]*[A-Fa-f])[0-9A-Fa-f]{12}\b", RegexOptions.CultureInvariant)]
    private static partial Regex MacCompactRegex();

    [GeneratedRegex(@"[^0-9A-Fa-f]", RegexOptions.CultureInvariant)]
    private static partial Regex HexOnlyRegex();

    [GeneratedRegex(@"^[0-9A-Fa-f]{12}$", RegexOptions.CultureInvariant)]
    private static partial Regex HexOnlyValidRegex();

    [GeneratedRegex(
        @"\b(?:Serial(?:\s*(?:number|no\.?|#))?|S/?N)\s*[:#]?\s*([A-Za-z0-9][A-Za-z0-9\-_/]{3,79})\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex SerialLabelRegex();

    [GeneratedRegex(@"\b[A-Za-z0-9][A-Za-z0-9\-_/]{4,39}\b", RegexOptions.CultureInvariant)]
    private static partial Regex SerialTokenRegex();

    [GeneratedRegex(@"\b\d{2}(?:\.\d{2}){2,}(?:-\d+)?\b", RegexOptions.CultureInvariant)]
    private static partial Regex FirmwareVersionRegex();

    [GeneratedRegex(@"\bET?585\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex E585Regex();

    [GeneratedRegex(@"\bET?580\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex E580Regex();

    [GeneratedRegex(@"\bY6(?:\s*Pro)?\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex Y6Regex();
}
