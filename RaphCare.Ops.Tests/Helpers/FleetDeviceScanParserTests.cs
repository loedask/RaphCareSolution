using RaphCare.Client.Models.Fleet;
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
    public void Parse_DeviceInfoOcr_CompactMacWithoutColons_IsMacNotSerial()
    {
        // OCR often drops colons; series label ET595 must not become the packaging serial.
        const string ocr =
            """
            Device Info
            ET595
            MAC
            6F9ACBACE445
            Version
            00.53.04.00-5291
            TP
            04.00.00.00.00.00
            """;

        var result = FleetDeviceScanParser.Parse(ocr);
        var fill = FleetDeviceScanParser.SuggestFill(result);

        Assert.Contains(
            result.Candidates,
            c => string.Equals(c.Kind, "Mac", StringComparison.OrdinalIgnoreCase)
                 && c.Value == "6F:9A:CB:AC:E4:45");
        Assert.DoesNotContain(
            result.Candidates,
            c => string.Equals(c.Kind, "Serial", StringComparison.OrdinalIgnoreCase));
        Assert.Null(fill.Serial);
        Assert.Equal("6F:9A:CB:AC:E4:45", fill.Mac);
        Assert.True(result.PreferredLooksLikeMac);
        Assert.Equal("E585", result.SuggestedModel);
    }

    [Fact]
    public void Parse_DeviceInfoOcr_Et585NearMiss_MapsModelAndSkipsSeriesSerial()
    {
        var result = FleetDeviceScanParser.Parse("Device Info\nET595\nMAC\nAA:BB:CC:DD:EE:FF");
        var fill = FleetDeviceScanParser.SuggestFill(result);

        Assert.Equal("E585", result.SuggestedModel);
        Assert.Null(fill.Serial);
        Assert.Equal("AA:BB:CC:DD:EE:FF", fill.Mac);
    }

    [Fact]
    public void SuggestFill_PackagingSerialAndMac_FillsBothFields()
    {
        const string ocr =
            """
            Model E580
            Serial number: RC-E580-2044
            MAC 11:22:33:44:55:66
            """;

        var fill = FleetDeviceScanParser.SuggestFill(FleetDeviceScanParser.Parse(ocr));

        Assert.Equal("RC-E580-2044", fill.Serial);
        Assert.Equal("11:22:33:44:55:66", fill.Mac);
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
    public void Parse_AllDigitTwelveHex_IsNotTreatedAsMac()
    {
        // TP blobs can OCR as 12 digits; those must not become Bluetooth MAC.
        var result = FleetDeviceScanParser.Parse("TP 040000000000 Version 00.53.04.00");

        Assert.DoesNotContain(
            result.Candidates,
            c => string.Equals(c.Kind, "Mac", StringComparison.OrdinalIgnoreCase));
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
