using System.Text;
using System.Text.RegularExpressions;

namespace RaphCare.Application.Common.Devices;

/// <summary>Normalize and validate Bluetooth MAC addresses for fleet bind and stock.</summary>
public static partial class BluetoothMacAddress
{
    public const int MaxLength = 17;

    /// <summary>
    /// Returns a canonical <c>AA:BB:CC:DD:EE:FF</c> form, or null when <paramref name="value"/> is blank.
    /// Throws <see cref="FormatException"/> when non-blank input is not a 6-byte hex MAC.
    /// </summary>
    public static string? NormalizeOrNull(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var hex = HexOnly().Replace(value.Trim(), string.Empty);
        if (hex.Length != 12 || !HexOnlyValid().IsMatch(hex))
            throw new FormatException("Bluetooth MAC must be 6 bytes of hex (for example AA:BB:CC:DD:EE:FF).");

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

    /// <summary>True when both sides normalize to the same MAC (blank never matches a set MAC).</summary>
    public static bool EqualsNormalized(string? left, string? right)
    {
        try
        {
            var a = NormalizeOrNull(left);
            var b = NormalizeOrNull(right);
            if (a is null || b is null)
                return false;
            return string.Equals(a, b, StringComparison.Ordinal);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    [GeneratedRegex(@"[^0-9A-Fa-f]", RegexOptions.CultureInvariant)]
    private static partial Regex HexOnly();

    [GeneratedRegex(@"^[0-9A-Fa-f]{12}$", RegexOptions.CultureInvariant)]
    private static partial Regex HexOnlyValid();
}
