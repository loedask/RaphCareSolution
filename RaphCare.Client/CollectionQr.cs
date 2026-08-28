namespace RaphCare.Client;

/// <summary>Pickup-code and wall-poster QR payloads for collection.</summary>
public static class CollectionQr
{
    public const string PosterScheme = "raphcare";
    public const string PosterHost = "collect";
    public const string PickupAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public static string ForPoster(Guid clinicId) =>
        $"{PosterScheme}://{PosterHost}/{clinicId:D}";

    public static bool TryParsePoster(string? raw, out Guid clinicId)
    {
        clinicId = default;
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        var text = raw.Trim();
        if (Uri.TryCreate(text, UriKind.Absolute, out var uri)
            && string.Equals(uri.Scheme, PosterScheme, StringComparison.OrdinalIgnoreCase)
            && string.Equals(uri.Host, PosterHost, StringComparison.OrdinalIgnoreCase)
            && Guid.TryParse(uri.AbsolutePath.Trim('/'), out clinicId))
            return true;

        const string prefix = "RAPHCARE-COLLECT:";
        if (text.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            && Guid.TryParse(text[prefix.Length..].Trim(), out clinicId))
            return true;

        return false;
    }

    public static bool TryParsePickupCode(string? raw, out string code)
    {
        code = string.Empty;
        if (TryParsePoster(raw, out _))
            return false;
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        var letters = new string((raw ?? string.Empty)
            .Trim()
            .ToUpperInvariant()
            .Where(c => PickupAlphabet.Contains(c))
            .ToArray());
        if (letters.Length < 6)
            return false;

        code = letters[..6];
        return true;
    }
}
