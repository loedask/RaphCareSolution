using System.Globalization;

namespace RaphCare.Mobile.Core.Features.Settings.Services;

/// <summary>Preferences-backed store for profile fields and privacy switches.</summary>
public sealed class LocalPatientProfileStore : ILocalPatientProfileStore
{
    private const string Prefix = "v12_profile_";

    public string FirstName
    {
        get => Preferences.Get(Prefix + nameof(FirstName), string.Empty);
        set => Preferences.Set(Prefix + nameof(FirstName), value ?? string.Empty);
    }

    public string LastName
    {
        get => Preferences.Get(Prefix + nameof(LastName), string.Empty);
        set => Preferences.Set(Prefix + nameof(LastName), value ?? string.Empty);
    }

    public string Email
    {
        get => Preferences.Get(Prefix + nameof(Email), string.Empty);
        set => Preferences.Set(Prefix + nameof(Email), value ?? string.Empty);
    }

    public string Phone
    {
        get => Preferences.Get(Prefix + nameof(Phone), string.Empty);
        set => Preferences.Set(Prefix + nameof(Phone), value ?? string.Empty);
    }

    public DateTime? DateOfBirth
    {
        get
        {
            var s = Preferences.Get(Prefix + nameof(DateOfBirth), string.Empty);
            return DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d.Date : null;
        }
        set => Preferences.Set(Prefix + nameof(DateOfBirth), value?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty);
    }

    public string Gender
    {
        get => Preferences.Get(Prefix + nameof(Gender), string.Empty);
        set => Preferences.Set(Prefix + nameof(Gender), value ?? string.Empty);
    }

    public bool PrivacyDataSharing
    {
        get => Preferences.Get(Prefix + nameof(PrivacyDataSharing), true);
        set => Preferences.Set(Prefix + nameof(PrivacyDataSharing), value);
    }

    public bool PrivacyTwoFactor
    {
        get => Preferences.Get(Prefix + nameof(PrivacyTwoFactor), false);
        set => Preferences.Set(Prefix + nameof(PrivacyTwoFactor), value);
    }
}
