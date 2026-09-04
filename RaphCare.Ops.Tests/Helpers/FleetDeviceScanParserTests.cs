using RaphCare.Ops.Helpers;
using Xunit;

namespace RaphCare.Ops.Tests.Helpers;

public sealed class FleetDeviceScanParserTests
{
    [Fact]
    public void Parse_DeviceInfoScreen_PrefersMacAndSetsE585()
    {
        const string ocr =
            """
            Device Info
            ET585
            MAC
            6F:9A:C8:4C:E4:45
            Version
            00.53.04.00-5291
            TP
            04.00.00.00.00.00
            """;

        var result = FleetDeviceScanParser.Parse(ocr);

        Assert.Equal("E585", result.SuggestedModel);
        Assert.Equal("6F:9A:C8:4C:E4:45", result.PreferredValue);
        Assert.True(result.PreferredLooksLikeMac);
        Assert.DoesNotContain(result.Candidates, c => c.Value.Contains("Device", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(result.Candidates, c => c.Value.StartsWith("00.53", StringComparison.Ordinal));
    }

    [Fact]
    public void Parse_PackagingSerialLabel_PrefersSerialOverNearbyMac()
    {
        const string ocr =
            """
            Model E580
            Serial number: RC-E580-2044
            MAC 11:22:33:44:55:66
            """;

        var result = FleetDeviceScanParser.Parse(ocr);

        Assert.Equal("E580", result.SuggestedModel);
        Assert.Equal("RC-E580-2044", result.PreferredValue);
        Assert.False(result.PreferredLooksLikeMac);
    }

    [Fact]
    public void ParseBarcode_UsesRawPayloadAsSerial()
    {
        var result = FleetDeviceScanParser.ParseBarcode("RC-E585-1001");

        Assert.Equal("RC-E585-1001", result.PreferredValue);
        Assert.False(result.PreferredLooksLikeMac);
    }

    [Fact]
    public void ParseBarcode_NumericOnlyPayload_IsAcceptedAsSerial()
    {
        var result = FleetDeviceScanParser.ParseBarcode("8901234567890");

        Assert.Equal("8901234567890", result.PreferredValue);
        Assert.False(result.PreferredLooksLikeMac);
    }

    [Fact]
    public void Parse_Y6Pro_DetectsModel()
    {
        var result = FleetDeviceScanParser.Parse("Y6 Pro SN: Y6-7788-AA");

        Assert.Equal("Y6 Pro", result.SuggestedModel);
        Assert.Equal("Y6-7788-AA", result.PreferredValue);
    }

    [Fact]
    public void Parse_Empty_ReturnsNoCandidates()
    {
        var result = FleetDeviceScanParser.Parse("   ");

        Assert.Null(result.PreferredValue);
        Assert.Empty(result.Candidates);
    }
}
