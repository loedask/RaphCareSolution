namespace RaphCare.Mobile.Core.Shared.Services.FeatureFlags;

/// <summary>
/// Feature flags to enable/disable areas of the app while under development.
/// When a flag is false, navigating to that feature shows UnderConstructionPage.
/// </summary>
/// <remarks>
/// <para><b>Defaults</b> are defined in <c>appsettings.json</c> under <see cref="FeatureFlagOptions.SectionName"/>.</para>
/// <para>Override locally with <c>appsettings.Development.json</c> (optional) or .NET User Secrets (see <c>docs/09_Mobile_App_Guide.md</c>).</para>
    /// <para><b>User Secrets example:</b> <c>dotnet user-secrets set "FeatureFlags:RecordsEnabled" "true" --project RaphCare.Mobile</c> — also <c>CareTelehealthEnabled</c>, <c>DevicesEnabled</c>, <c>BillingEnabled</c>, etc.</para>
/// </remarks>
public static class FeatureFlags
{
    public static bool RecordsEnabled { get; private set; }
    public static bool AppointmentsEnabled { get; private set; }
    public static bool InsuranceEnabled { get; private set; }
    public static bool SettingsEnabled { get; private set; } = true;
    public static bool CareTelehealthEnabled { get; private set; }
    public static bool DevicesEnabled { get; private set; }
    public static bool BillingEnabled { get; private set; }
    public static bool MentalHealthEnabled { get; private set; }
    public static bool FamilyMembersEnabled { get; private set; }
    public static bool AiAssistantEnabled { get; private set; }
    public static bool NotificationsEnabled { get; private set; }

    /// <summary>
    /// Applies options from configuration (called once at startup after the MAUI app is built).
    /// </summary>
    public static void Initialize(FeatureFlagOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        RecordsEnabled = options.RecordsEnabled;
        AppointmentsEnabled = options.AppointmentsEnabled;
        InsuranceEnabled = options.InsuranceEnabled;
        SettingsEnabled = options.SettingsEnabled;
        CareTelehealthEnabled = options.CareTelehealthEnabled;
        DevicesEnabled = options.DevicesEnabled;
        BillingEnabled = options.BillingEnabled;
        MentalHealthEnabled = options.MentalHealthEnabled;
        FamilyMembersEnabled = options.FamilyMembersEnabled;
        AiAssistantEnabled = options.AiAssistantEnabled;
        NotificationsEnabled = options.NotificationsEnabled;
    }
}
