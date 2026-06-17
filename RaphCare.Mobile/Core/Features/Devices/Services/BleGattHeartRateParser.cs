using System.Buffers.Binary;

namespace RaphCare.Mobile.Core.Features.Devices.Services;

/// <summary>Bluetooth SIG Heart Rate Measurement characteristic (0x2A37).</summary>
public static class BleGattHeartRateParser
{
    /// <summary>Parses Bluetooth Heart Rate Measurement (flags + HR value).</summary>
    public static bool TryParseHeartRateMeasurement(ReadOnlySpan<byte> data, out int bpm)
    {
        bpm = 0;
        if (data.Length < 2)
            return false;

        var flags = data[0];
        if ((flags & 0x1) == 0)
        {
            bpm = data[1];
            return bpm > 0;
        }

        if (data.Length >= 3)
        {
            bpm = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(1));
            return bpm > 0;
        }

        return false;
    }

    public static string ToHex(ReadOnlySpan<byte> data)
    {
        if (data.Length == 0)
            return string.Empty;
        return Convert.ToHexString(data);
    }
}
