using System.Security.Cryptography;

namespace RaphCare.Domain.Organization;

/// <summary>
/// Platform-issued hospital reference (for example <c>RC-K7M3P2</c>). Distinct from the hospital's own registration number.
/// </summary>
public static class ClinicReferenceCode
{
    public const string Prefix = "RC-";
    public const int BodyLength = 6;
    public const int MaxLength = 12;

    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    /// <summary>Creates a new random reference. Uniqueness is enforced by persistence.</summary>
    public static string Generate()
    {
        Span<char> body = stackalloc char[BodyLength];
        var bytes = RandomNumberGenerator.GetBytes(BodyLength);
        for (var i = 0; i < BodyLength; i++)
            body[i] = Alphabet[bytes[i] % Alphabet.Length];

        return Prefix + new string(body);
    }

    /// <summary>
    /// Returns true when <paramref name="value"/> is a well-formed reference after trimming and casing.
    /// Does not treat official registration numbers as references.
    /// </summary>
    public static bool TryNormalize(string? value, out string code)
    {
        code = string.Empty;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var compact = new string(value
            .Where(static c => !char.IsWhiteSpace(c))
            .Select(char.ToUpperInvariant)
            .ToArray());

        if (compact.Length == Prefix.Length - 1 + BodyLength
            && compact.StartsWith("RC", StringComparison.Ordinal)
            && (compact.Length < 3 || compact[2] != '-'))
        {
            compact = Prefix + compact[2..];
        }

        if (compact.Length != Prefix.Length + BodyLength
            || !compact.StartsWith(Prefix, StringComparison.Ordinal))
        {
            return false;
        }

        var body = compact.AsSpan(Prefix.Length);
        foreach (var ch in body)
        {
            if (!Alphabet.Contains(ch))
                return false;
        }

        code = compact;
        return true;
    }
}
