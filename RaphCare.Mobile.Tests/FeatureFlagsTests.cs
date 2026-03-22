using RaphCare.Mobile.Core.Shared.Services.FeatureFlags;
using Xunit;

namespace RaphCare.Mobile.Tests;

public class FeatureFlagsTests
{
    [Fact]
    public void Initialize_copies_options_to_static_flags()
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
    }

    [Fact]
    public void Initialize_throws_on_null_options()
    {
        Assert.Throws<ArgumentNullException>(() => FeatureFlags.Initialize(null!));
    }
}
