using System.Linq;
using System.Text;
using System.Text.Json;

namespace RaphCare.Mobile.Core.Features.Settings.Services;

/// <summary>Reads display-oriented claims from an access token payload without validating the JWT.</summary>
public static class JwtClaimsReader
{
    /// <summary>Returns a display name and email-like identifier when present in the payload.</summary>
    public static (string? Name, string? Email) ReadDisplayClaims(string? jwt)
    {
        if (string.IsNullOrWhiteSpace(jwt))
            return (null, null);

        var parts = jwt.Split('.');
        if (parts.Length < 2)
            return (null, null);

        try
        {
            var json = Encoding.UTF8.GetString(Base64UrlDecode(parts[1]));
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var name = TryGetString(root, "name");
            if (string.IsNullOrEmpty(name) && root.TryGetProperty("given_name", out var gn))
            {
                var g = gn.GetString();
                var f = root.TryGetProperty("family_name", out var fn) ? fn.GetString() : null;
                name = string.Join(" ", new[] { g, f }.Where(static s => !string.IsNullOrWhiteSpace(s)));
            }

            var email = TryGetString(root, "preferred_username")
                ?? TryGetString(root, "email")
                ?? TryFirstEmailFromArray(root, "emails");

            return (string.IsNullOrWhiteSpace(name) ? null : name.Trim(), string.IsNullOrWhiteSpace(email) ? null : email.Trim());
        }
        catch
        {
            return (null, null);
        }
    }

    private static string? TryGetString(JsonElement root, string name) =>
        root.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.String ? p.GetString() : null;

    private static string? TryFirstEmailFromArray(JsonElement root, string name)
    {
        if (!root.TryGetProperty(name, out var arr) || arr.ValueKind != JsonValueKind.Array)
            return null;
        foreach (var el in arr.EnumerateArray())
        {
            if (el.ValueKind == JsonValueKind.String)
                return el.GetString();
        }

        return null;
    }

    private static byte[] Base64UrlDecode(string input)
    {
        var s = input.Replace("-", "+", StringComparison.Ordinal).Replace("_", "/", StringComparison.Ordinal);
        return (s.Length % 4) switch
        {
            2 => Convert.FromBase64String(s + "=="),
            3 => Convert.FromBase64String(s + "="),
            _ => Convert.FromBase64String(s)
        };
    }
}
