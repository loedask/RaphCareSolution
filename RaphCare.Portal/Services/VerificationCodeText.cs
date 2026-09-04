namespace RaphCare.Portal.Services;

/// <summary>Normalizes pasted email verification codes (spaces from Outlook copy, etc.).</summary>
public static class VerificationCodeText
{
    public static string Normalize(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        var chars = value.Where(char.IsAsciiDigit).ToArray();
        return new string(chars);
    }

    public static string? NormalizeOrNull(string? value)
    {
        var normalized = Normalize(value);
        return normalized.Length == 0 ? null : normalized;
    }
}
