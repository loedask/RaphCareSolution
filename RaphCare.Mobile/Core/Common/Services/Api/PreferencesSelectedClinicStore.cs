using System.Globalization;

namespace RaphCare.Mobile.Core.Common.Services.Api;

/// <summary>Stores the selected clinic Guid, name, and reference code in preferences.</summary>
public sealed class PreferencesSelectedClinicStore : ISelectedClinicStore
{
    private const string Prefix = "SelectedClinic.";

    public Guid? ClinicId
    {
        get
        {
            var raw = Preferences.Get(Prefix + nameof(ClinicId), string.Empty);
            return Guid.TryParse(raw, out var id) && id != Guid.Empty ? id : null;
        }
    }

    public string? ClinicName
    {
        get
        {
            var name = Preferences.Get(Prefix + nameof(ClinicName), string.Empty);
            return string.IsNullOrWhiteSpace(name) ? null : name;
        }
    }

    public string? ReferenceCode
    {
        get
        {
            var code = Preferences.Get(Prefix + nameof(ReferenceCode), string.Empty);
            return string.IsNullOrWhiteSpace(code) ? null : code;
        }
    }

    public void SetClinic(Guid clinicId, string name, string referenceCode)
    {
        if (clinicId == Guid.Empty)
            throw new ArgumentException("Clinic id is required.", nameof(clinicId));

        Preferences.Set(Prefix + nameof(ClinicId), clinicId.ToString("D", CultureInfo.InvariantCulture));
        Preferences.Set(Prefix + nameof(ClinicName), name?.Trim() ?? string.Empty);
        Preferences.Set(Prefix + nameof(ReferenceCode), referenceCode?.Trim() ?? string.Empty);
    }

    public void Clear()
    {
        Preferences.Remove(Prefix + nameof(ClinicId));
        Preferences.Remove(Prefix + nameof(ClinicName));
        Preferences.Remove(Prefix + nameof(ReferenceCode));
    }
}
