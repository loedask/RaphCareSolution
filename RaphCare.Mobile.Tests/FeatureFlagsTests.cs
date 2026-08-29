using RaphCare.Mobile.Core.Common.Services.FeatureFlags;
using Xunit;

namespace RaphCare.Mobile.Tests;

public class FeatureFlagsTests
{
    [Fact]
    public void InitializeCopiesOptionsToStaticFlags()
    {
        var options = new FeatureFlagOptions
        {
            RecordsEnabled = true,
            AppointmentsEnabled = false,
            InsuranceEnabled = true,
            SettingsEnabled = false
        };

        FeatureFlags.Initialize(options);

        Assert.True(FeatureFlags.RecordsEnabled);
        Assert.False(FeatureFlags.AppointmentsEnabled);
        Assert.True(FeatureFlags.InsuranceEnabled);
        Assert.False(FeatureFlags.SettingsEnabled);
        Assert.False(FeatureFlags.CareTelehealthEnabled);
        Assert.False(FeatureFlags.DevicesEnabled);
        Assert.False(FeatureFlags.BillingEnabled);
        Assert.False(FeatureFlags.MentalHealthEnabled);
        Assert.False(FeatureFlags.FamilyMembersEnabled);
        Assert.False(FeatureFlags.AiAssistantEnabled);
        Assert.False(FeatureFlags.NotificationsEnabled);
        Assert.False(FeatureFlags.PhoneRegistrationEnabled);
        Assert.False(FeatureFlags.VoiceRegistrationEnabled);
    }

    [Fact]
    public void InitializeCopiesPhoneAndVoiceRegistrationFlags()
    {
        var options = new FeatureFlagOptions
        {
            PhoneRegistrationEnabled = true,
            VoiceRegistrationEnabled = true
        };

        FeatureFlags.Initialize(options);

        Assert.True(FeatureFlags.PhoneRegistrationEnabled);
        Assert.True(FeatureFlags.VoiceRegistrationEnabled);
    }

    [Fact]
    public void InitializeThrowsOnNullOptions()
    {
        Assert.Throws<ArgumentNullException>(() => FeatureFlags.Initialize(null!));
    }
}
