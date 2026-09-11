using RaphCare.Mobile.Kernel.Core.Common.Devices;

namespace RaphCare.Mobile.Core.Features.Devices.HBand;

/// <summary>
/// File-backed Connect step probe. Flushes to disk so a native abort still leaves a breadcrumb.
/// </summary>
public sealed class FileVendorConnectStepProbe : IVendorConnectStepProbe
{
    private readonly object _sync = new();
    private readonly string _path;

    public FileVendorConnectStepProbe()
        : this(Path.Combine(FileSystem.Current.AppDataDirectory, "vendor_connect_step_probe.txt"))
    {
    }

    /// <summary>Test seam.</summary>
    public FileVendorConnectStepProbe(string path) =>
        _path = path ?? throw new ArgumentNullException(nameof(path));

    public void Mark(string stepCode)
    {
        if (string.IsNullOrWhiteSpace(stepCode))
            return;

        lock (_sync)
        {
            if (VendorConnectCrashProbeRules.ClearsProbe(stepCode))
            {
                TryDeleteUnlocked();
                return;
            }

            WriteThroughUnlocked(stepCode.Trim());
        }
    }

    public void Clear()
    {
        lock (_sync)
            TryDeleteUnlocked();
    }

    public string? TryConsumeIncompleteStep()
    {
        lock (_sync)
        {
            if (!File.Exists(_path))
                return null;

            string? step;
            try
            {
                step = File.ReadAllText(_path).Trim();
            }
            catch
            {
                TryDeleteUnlocked();
                return null;
            }

            TryDeleteUnlocked();
            return VendorConnectCrashProbeRules.ShouldReportIncompleteStep(step) ? step : null;
        }
    }

    private void WriteThroughUnlocked(string step)
    {
        var dir = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        using var fs = new FileStream(
            _path,
            FileMode.Create,
            FileAccess.Write,
            FileShare.Read,
            bufferSize: 256,
            FileOptions.WriteThrough);
        using var writer = new StreamWriter(fs);
        writer.Write(step);
        writer.Flush();
        fs.Flush(flushToDisk: true);
    }

    private void TryDeleteUnlocked()
    {
        try
        {
            if (File.Exists(_path))
                File.Delete(_path);
        }
        catch
        {
            // Best-effort clear.
        }
    }
}
