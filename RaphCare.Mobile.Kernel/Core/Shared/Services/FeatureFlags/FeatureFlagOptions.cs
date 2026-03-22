namespace RaphCare.Mobile.Core.Shared.Services.FeatureFlags;

/// <summary>
/// Bound from configuration section <see cref="SectionName"/>. Applied to <see cref="FeatureFlags"/> at startup.
/// </summary>
public class FeatureFlagOptions
{
    public const string SectionName = "FeatureFlags";

    public bool RecordsEnabled { get; set; }
    public bool AppointmentsEnabled { get; set; }
    public bool InsuranceEnabled { get; set; }
    public bool SettingsEnabled { get; set; } = true;
}
