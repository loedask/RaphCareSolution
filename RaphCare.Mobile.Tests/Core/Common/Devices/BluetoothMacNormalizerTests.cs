using RaphCare.Mobile.Kernel.Core.Common.Devices;
using Xunit;

namespace RaphCare.Mobile.Tests.Core.Common.Devices;

public sealed class BluetoothMacNormalizerTests
{
    [Fact]
    public void TryNormalizeMatchesApiCanonicalForm()
    {
        Assert.Equal("AA:BB:CC:DD:EE:FF", BluetoothMacNormalizer.TryNormalize("aabbccddeeff"));
        Assert.Null(BluetoothMacNormalizer.TryNormalize("bad"));
    }

    [Fact]
    public void EqualsNormalizedRejectsMismatch()
    {
        Assert.True(BluetoothMacNormalizer.EqualsNormalized("AA:BB:CC:DD:EE:FF", "aa-bb-cc-dd-ee-ff"));
        Assert.False(BluetoothMacNormalizer.EqualsNormalized("AA:BB:CC:DD:EE:FF", "11:22:33:44:55:66"));
    }
}
