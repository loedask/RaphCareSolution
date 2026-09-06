using System.Text;
using System.Text.RegularExpressions;

namespace RaphCare.Mobile.Kernel.Core.Common.Devices;

/// <summary>Normalize Bluetooth MAC for claim-bound Connect checks (matches API canonical form).</summary>
public static partial class BluetoothMacNormalizer
{
    /// <summary>Returns <c>AA:BB:CC:DD:EE:FF</c>, or null when blank / invalid.</summary>
    public static string? TryNormalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var hex = HexOnly().Replace(value.Trim(), string.Empty);
        if (hex.Length != 12 || !HexOnlyValid().IsMatch(hex))
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

    public static bool EqualsNormalized(string? left, string? right)
    {
        var a = TryNormalize(left);
        var b = TryNormalize(right);
        return a is not null && b is not null && string.Equals(a, b, StringComparison.Ordinal);
    }

    [GeneratedRegex(@"[^0-9A-Fa-f]", RegexOptions.CultureInvariant)]
    private static partial Regex HexOnly();

    [GeneratedRegex(@"^[0-9A-Fa-f]{12}$", RegexOptions.CultureInvariant)]
    private static partial Regex HexOnlyValid();
}
