using RaphCare.Application.Common.Devices;
using Xunit;

namespace RaphCare.Application.Tests.Common.Devices;

public sealed class BluetoothMacAddressTests
{
    [Theory]
    [InlineData("aa:bb:cc:dd:ee:ff", "AA:BB:CC:DD:EE:FF")]
    [InlineData("AABBCCDDEEFF", "AA:BB:CC:DD:EE:FF")]
    [InlineData("aa-bb-cc-dd-ee-ff", "AA:BB:CC:DD:EE:FF")]
    public void NormalizeAcceptsCommonForms(string input, string expected)
    {
        Assert.Equal(expected, BluetoothMacAddress.NormalizeOrNull(input));
    }

    [Fact]
    public void NormalizeBlankReturnsNull()
    {
        Assert.Null(BluetoothMacAddress.NormalizeOrNull("  "));
        Assert.Null(BluetoothMacAddress.NormalizeOrNull(null));
    }

    [Fact]
    public void NormalizeInvalidThrows()
    {
        Assert.Throws<FormatException>(() => BluetoothMacAddress.NormalizeOrNull("not-a-mac"));
        Assert.Throws<FormatException>(() => BluetoothMacAddress.NormalizeOrNull("AA:BB"));
    }

    [Fact]
    public void EqualsNormalizedMatchesAcrossFormats()
    {
        Assert.True(BluetoothMacAddress.EqualsNormalized("aa:bb:cc:dd:ee:ff", "AABBCCDDEEFF"));
        Assert.False(BluetoothMacAddress.EqualsNormalized("aa:bb:cc:dd:ee:ff", "11:22:33:44:55:66"));
        Assert.False(BluetoothMacAddress.EqualsNormalized(null, "aa:bb:cc:dd:ee:ff"));
    }
}
