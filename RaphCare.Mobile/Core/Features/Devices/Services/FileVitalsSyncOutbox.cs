using System.Text.Json;
using Microsoft.Maui.Storage;
using RaphCare.Client.Contracts.Interfaces;
using RaphCare.Client.Models.Devices;

namespace RaphCare.Mobile.Core.Features.Devices.Services;

/// <summary>Stores pending vitals batches as JSON under the app data directory (max <see cref="MaxBatches"/>).</summary>
public sealed class FileVitalsSyncOutbox : IVitalsSyncOutbox, IDisposable
{
    internal const int MaxBatches = 50;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
    };

    private readonly SemaphoreSlim _gate = new(1, 1);

    private static string StorePath =>
        Path.Combine(FileSystem.Current.AppDataDirectory, "vitals_sync_outbox.json");

    /// <inheritdoc />
    public async Task EnqueueAsync(
        Guid deviceId,
        IReadOnlyList<HeartRateReadingInput> heartRates,
        IReadOnlyList<Spo2ReadingInput> spo2Readings,
        CancellationToken cancellationToken = default)
    {
        await WithLockAsync(
                async () =>
                {
                    var batches = await ReadAllAsync(cancellationToken).ConfigureAwait(false);
                    batches.Add(
                        new PendingVitalsBatchDto
                        {
                            Id = Guid.NewGuid(),
                            DeviceId = deviceId,
                            HeartRates = heartRates.Select(h => new HeartRateReadingInput
                            {
                                RecordedAt = h.RecordedAt,
                                BeatsPerMinute = h.BeatsPerMinute,
                            }).ToList(),
                            SpO2Readings = spo2Readings.Select(s => new Spo2ReadingInput
                            {
                                RecordedAt = s.RecordedAt,
                                SpO2 = s.SpO2,
                                PulseRate = s.PulseRate,
                            }).ToList(),
                            EnqueuedAt = DateTimeOffset.UtcNow,
                        });

                    while (batches.Count > MaxBatches)
                        batches.RemoveAt(0);

                    await WriteAllAsync(batches, cancellationToken).ConfigureAwait(false);
                },
                cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<int> TryFlushAsync(IPatientDevicesService patientDevices, CancellationToken cancellationToken = default)
    {
        return await WithLockAsync(
                async () =>
                {
                    var batches = await ReadAllAsync(cancellationToken).ConfigureAwait(false);
                    if (batches.Count == 0)
                        return 0;

                    var remaining = new List<PendingVitalsBatchDto>();
                    var successCount = 0;

                    foreach (var batch in batches)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var resp = await patientDevices.SyncReadingsAsync(
                                batch.DeviceId,
                                batch.HeartRates,
                                batch.SpO2Readings,
                                cancellationToken)
                            .ConfigureAwait(false);

                        if (resp.IsSuccess)
                            successCount++;
                        else
                            remaining.Add(batch);
                    }

                    await WriteAllAsync(remaining, cancellationToken).ConfigureAwait(false);
                    return successCount;
                },
                cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task WithLockAsync(Func<Task> work, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await work().ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<T> WithLockAsync<T>(Func<Task<T>> work, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await work().ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    private static async Task<List<PendingVitalsBatchDto>> ReadAllAsync(CancellationToken cancellationToken)
    {
        var path = StorePath;
        if (!File.Exists(path))
            return [];

        await using var stream = File.OpenRead(path);
        var list = await JsonSerializer.DeserializeAsync<List<PendingVitalsBatchDto>>(stream, JsonOptions, cancellationToken)
            .ConfigureAwait(false);
        return list ?? [];
    }

    private static async Task WriteAllAsync(IReadOnlyList<PendingVitalsBatchDto> batches, CancellationToken cancellationToken)
    {
        var path = StorePath;
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, batches, JsonOptions, cancellationToken).ConfigureAwait(false);
    }

    private sealed class PendingVitalsBatchDto
    {
        public Guid Id { get; set; }
        public Guid DeviceId { get; set; }
        public List<HeartRateReadingInput> HeartRates { get; set; } = [];
        public List<Spo2ReadingInput> SpO2Readings { get; set; } = [];
        public DateTimeOffset EnqueuedAt { get; set; }
    }

    public void Dispose() => _gate.Dispose();
}
