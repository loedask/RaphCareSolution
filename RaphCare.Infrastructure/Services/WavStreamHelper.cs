namespace RaphCare.Infrastructure.Services;

/// <summary>Materializes audio input as a seekable in-memory WAV stream for STT providers.</summary>
internal static class WavStreamHelper
{
    public static async Task<MemoryStream> MaterializeWavAsync(Stream audioStream, CancellationToken cancellationToken)
    {
        var memory = new MemoryStream();
        if (audioStream.CanSeek)
            audioStream.Position = 0;

        await audioStream.CopyToAsync(memory, cancellationToken).ConfigureAwait(false);
        memory.Position = 0;
        return memory;
    }
}
