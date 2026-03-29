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

    /// <summary>Request call, consultation, in-app / PSTN care (concept: /request-call, /consultation).</summary>
    public bool CareTelehealthEnabled { get; set; }

    /// <summary>Connected wearables — BLE (e.g. E585/E580) and related device UX (concept: /devices).</summary>
    public bool DevicesEnabled { get; set; }

    /// <summary>Payment methods, billing history, plan upgrade (concept billing routes).</summary>
    public bool BillingEnabled { get; set; }

    public bool MentalHealthEnabled { get; set; }
    public bool FamilyMembersEnabled { get; set; }
    public bool AiAssistantEnabled { get; set; }
    public bool NotificationsEnabled { get; set; }
}
