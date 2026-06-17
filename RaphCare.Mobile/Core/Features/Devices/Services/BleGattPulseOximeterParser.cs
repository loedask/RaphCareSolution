using System.Buffers.Binary;

namespace RaphCare.Mobile.Core.Features.Devices.Services;

/// <summary>
/// Bluetooth SIG Pulse Oximeter (PLX) characteristics: Continuous (0x2A60) and Spot-check (0x2A5F), SFLOAT per IEEE 11073-20601.
/// </summary>
public static class BleGattPulseOximeterParser
{
    /// <summary>PLX Continuous Measurement — flags bit0 = SpO₂ and pulse rate present as two little-endian SFLOATs.</summary>
    public static bool TryParsePlxContinuousMeasurement(ReadOnlySpan<byte> data, out decimal spo2Percent, out int? pulseBpm)
    {
        spo2Percent = 0;
        pulseBpm = null;
        if (data.Length < 5)
            return false;
        if ((data[0] & 0x01) == 0)
            return false;
        return TryReadSpo2AndPulseSfloats(data.Slice(1, 4), out spo2Percent, out pulseBpm);
    }

    /// <summary>
    /// PLX Spot-check — optional 7-byte timestamp when flags bit0 is set; then SpO₂ and PR as SFLOATs.
    /// </summary>
    public static bool TryParsePlxSpotCheckMeasurement(ReadOnlySpan<byte> data, out decimal spo2Percent, out int? pulseBpm)
    {
        spo2Percent = 0;
        pulseBpm = null;
        if (data.Length < 5)
            return false;

        var offset = 1;
        if ((data[0] & 0x01) != 0)
        {
            if (data.Length < 1 + 7)
                return false;
            offset += 7;
        }

        if (data.Length < offset + 4)
            return false;

        return TryReadSpo2AndPulseSfloats(data.Slice(offset, 4), out spo2Percent, out pulseBpm);
    }

    private static bool TryReadSpo2AndPulseSfloats(ReadOnlySpan<byte> fourBytes, out decimal spo2Percent, out int? pulseBpm)
    {
        spo2Percent = 0;
        pulseBpm = null;
        if (fourBytes.Length < 4)
            return false;

        var rawSpo2 = BinaryPrimitives.ReadUInt16LittleEndian(fourBytes[..2]);
        var rawPr = BinaryPrimitives.ReadUInt16LittleEndian(fourBytes.Slice(2, 2));

        if (!TrySFloatToDecimal(rawSpo2, out var sp) || sp < 50m || sp > 100m)
            return false;

        spo2Percent = decimal.Round(sp, 1, MidpointRounding.AwayFromZero);

        if (TrySFloatToDecimal(rawPr, out var pr) && pr >= 30m && pr <= 220m)
            pulseBpm = (int)Math.Round(pr, MidpointRounding.AwayFromZero);

        return true;
    }

    /// <summary>16-bit IEEE 11073-20601 special float (mantissa 12 bits, exponent 4 bits).</summary>
    public static bool TrySFloatToDecimal(ushort raw, out decimal value)
    {
        value = 0;
        var mantissa = raw & 0x0FFF;
        if (mantissa >= 0x0800)
            mantissa -= 0x1000;

        var exponent = (raw >> 12) & 0x000F;
        if (exponent >= 0x0008)
            exponent -= 0x0010;

        if (mantissa == 0 && (exponent == 0 || exponent == -15))
        {
            value = 0;
            return false;
        }

        value = mantissa * (decimal)Math.Pow(10, exponent);
        return true;
    }
}
