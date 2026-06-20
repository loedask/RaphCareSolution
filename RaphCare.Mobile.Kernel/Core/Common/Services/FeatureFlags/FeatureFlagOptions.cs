namespace RaphCare.Mobile.Core.Common.Services.FeatureFlags;

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

    /// <summary>Request call, consultation, in-app / PSTN care (concept: /request-call, /consultation).</summary>
    public bool CareTelehealthEnabled { get; set; }

    /// <summary>Connected wearables — BLE fleet (E580, E585; HBand SDK). See docs/13_Patient_Device_Packages_and_Fleet.md. Y6 Pro (4G emergency) uses a different integration path.</summary>
    public bool DevicesEnabled { get; set; }

    /// <summary>Payment methods, billing history, plan upgrade (concept billing routes).</summary>
    public bool BillingEnabled { get; set; }

    public bool MentalHealthEnabled { get; set; }
    public bool FamilyMembersEnabled { get; set; }
    public bool AiAssistantEnabled { get; set; }
    public bool NotificationsEnabled { get; set; }
}
