namespace RaphCare.Mobile.Core.Features.Auth.Models;

/// <summary>Dial codes aligned with the React concept register flow.</summary>
public sealed class CountryDialOption
{
    public CountryDialOption(string dialCode, string name)
    {
        DialCode = dialCode;
        Name = name;
    }

    public string DialCode { get; }
    public string Name { get; }

    public string Display => $"{Name} ({DialCode})";

    public static IReadOnlyList<CountryDialOption> DefaultList { get; } =
    [
        new("+234", "Nigeria"),
        new("+254", "Kenya"),
        new("+233", "Ghana"),
        new("+27", "South Africa"),
        new("+1", "United States"),
        new("+44", "United Kingdom"),
        new("+91", "India"),
    ];
}
