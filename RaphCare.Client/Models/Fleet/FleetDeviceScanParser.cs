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

        foreach (Match match in MacRegex().Matches(text))
            Add(NormalizeMac(match.Value), "Mac");

        foreach (Match match in SerialLabelRegex().Matches(text))
            Add(match.Groups[1].Value, "Serial");

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
            var kind = LooksLikeMac(value) ? "Mac" : "Serial";
            return new Result(
                DetectModel(value),
                LooksLikeMac(value) ? NormalizeMac(value) : value,
                LooksLikeMac(value),
                [new Candidate(LooksLikeMac(value) ? NormalizeMac(value) : value, kind)]);
        }

        return parsed;
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

    private static string NormalizeMac(string mac) =>
        mac.Replace('-', ':').ToUpperInvariant();

    private static bool LooksLikeMac(string value) => MacRegex().IsMatch(value.Trim());

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
    private static partial Regex MacRegex();

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
