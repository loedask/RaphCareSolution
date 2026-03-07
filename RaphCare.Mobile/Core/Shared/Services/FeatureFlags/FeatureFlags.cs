namespace RaphCare.Mobile.Core.Shared.Services.FeatureFlags;

/// <summary>
/// Feature flags to enable/disable areas of the app while under development.
/// When a flag is false, navigating to that feature shows UnderConstructionPage.
/// </summary>
public static class FeatureFlags
{
    public static bool RecordsEnabled { get; set; } = false;
    public static bool AppointmentsEnabled { get; set; } = false;
    public static bool InsuranceEnabled { get; set; } = false;
    public static bool SettingsEnabled { get; set; } = true;
}
