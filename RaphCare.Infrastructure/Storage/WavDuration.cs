namespace RaphCare.Infrastructure.Storage;

/// <summary>Reads duration from a PCM WAV header when present.</summary>
internal static class WavDuration
{
    public static int TryGetSeconds(Stream stream)
    {
        if (!stream.CanSeek)
            return 0;

        var original = stream.Position;
        try
        {
            if (stream.Length - original < 44)
                return 0;

            Span<byte> header = stackalloc byte[44];
            var read = stream.Read(header);
            if (read < 44)
                return 0;

            if (header[0] != (byte)'R' || header[1] != (byte)'I' || header[2] != (byte)'F' || header[3] != (byte)'F')
                return 0;
            if (header[8] != (byte)'W' || header[9] != (byte)'A' || header[10] != (byte)'V' || header[11] != (byte)'E')
                return 0;

            var channels = BitConverter.ToInt16(header.Slice(22, 2));
            var sampleRate = BitConverter.ToInt32(header.Slice(24, 4));
            var bitsPerSample = BitConverter.ToInt16(header.Slice(34, 2));
            var dataSize = BitConverter.ToInt32(header.Slice(40, 4));
            if (channels <= 0 || sampleRate <= 0 || bitsPerSample <= 0 || dataSize <= 0)
                return 0;

            var bytesPerSecond = sampleRate * channels * (bitsPerSample / 8);
            if (bytesPerSecond <= 0)
                return 0;

            return Math.Max(1, (int)Math.Round(dataSize / (double)bytesPerSecond));
        }
        catch (IOException)
        {
            return 0;
        }
        finally
        {
            stream.Position = original;
        }
    }
}
