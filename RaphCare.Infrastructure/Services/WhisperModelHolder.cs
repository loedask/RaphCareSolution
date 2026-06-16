using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaphCare.Application.Common.Configuration;
using Whisper.net;
using Whisper.net.Ggml;

namespace RaphCare.Infrastructure.Services;

/// <summary>Loads and caches the Whisper ggml model (singleton — model load is expensive).</summary>
public sealed class WhisperModelHolder(
    IOptionsMonitor<SpeechToTextOptions> options,
    ILogger<WhisperModelHolder> logger) : IAsyncDisposable
{
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private WhisperFactory? _factory;

    public async ValueTask<WhisperFactory> GetFactoryAsync(CancellationToken cancellationToken)
    {
        if (_factory is not null)
            return _factory;

        await _initLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_factory is not null)
                return _factory;

            var whisperOptions = options.CurrentValue.Whisper;
            var modelPath = ResolveModelPath(whisperOptions);

            if (!File.Exists(modelPath))
            {
                if (!whisperOptions.AutoDownloadModel)
                {
                    throw new FileNotFoundException(
                        $"Whisper model not found at '{modelPath}'. Set SpeechToText:Whisper:ModelPath or enable AutoDownloadModel.",
                        modelPath);
                }

                logger.LogInformation("Downloading Whisper ggml model ({ModelType}) to {Path}…", whisperOptions.ModelType, modelPath);
                Directory.CreateDirectory(Path.GetDirectoryName(modelPath)!);

                var ggmlType = ParseGgmlType(whisperOptions.ModelType);
                await using var modelStream = await WhisperGgmlDownloader.Default.GetGgmlModelAsync(ggmlType, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                await using var fileWriter = File.OpenWrite(modelPath);
                await modelStream.CopyToAsync(fileWriter, cancellationToken).ConfigureAwait(false);
                logger.LogInformation("Whisper model ready at {Path}.", modelPath);
            }

            _factory = WhisperFactory.FromPath(modelPath);
            return _factory;
        }
        finally
        {
            _initLock.Release();
        }
    }

    public ValueTask DisposeAsync()
    {
        _factory?.Dispose();
        _factory = null;
        return ValueTask.CompletedTask;
    }

    private static string ResolveModelPath(WhisperSpeechToTextOptions whisperOptions)
    {
        if (!string.IsNullOrWhiteSpace(whisperOptions.ModelPath))
            return whisperOptions.ModelPath;

        var directory = string.IsNullOrWhiteSpace(whisperOptions.ModelDirectory)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RaphCare", "whisper")
            : whisperOptions.ModelDirectory;

        var fileName = $"ggml-{whisperOptions.ModelType.Trim().ToLowerInvariant()}.bin";
        return Path.Combine(directory, fileName);
    }

    private static GgmlType ParseGgmlType(string modelType) =>
        Enum.TryParse<GgmlType>(modelType, ignoreCase: true, out var parsed)
            ? parsed
            : GgmlType.Base;
}
