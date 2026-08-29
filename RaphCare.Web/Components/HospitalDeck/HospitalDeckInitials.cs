namespace RaphCare.Web.Components.HospitalDeck;

internal static class HospitalDeckInitials
{
    public static string FromName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "?";

        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
            return string.Concat(char.ToUpperInvariant(parts[0][0]), char.ToUpperInvariant(parts[^1][0]));

        var token = parts[0];
        return token.Length >= 2
            ? token[..2].ToUpperInvariant()
            : char.ToUpperInvariant(token[0]).ToString();
    }
}
