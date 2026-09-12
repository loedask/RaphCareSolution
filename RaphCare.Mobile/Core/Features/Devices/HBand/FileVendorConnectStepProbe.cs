using RaphCare.Mobile.Kernel.Core.Common.Devices;

namespace RaphCare.Mobile.Core.Features.Devices.HBand;

/// <summary>
/// File-backed Connect step probe. Flushes to disk so a native abort still leaves a breadcrumb.
/// Also appends every step to a trail file the user can Share from the phone (no adb).
/// </summary>
public sealed class FileVendorConnectStepProbe : IVendorConnectStepProbe
{
    private readonly object _sync = new();
    private readonly string _path;
    private readonly string _trailPath;
    private readonly string? _mirrorTrailPath;

    public FileVendorConnectStepProbe()
        : this(
            Path.Combine(FileSystem.Current.AppDataDirectory, "vendor_connect_step_probe.txt"),
            Path.Combine(FileSystem.Current.AppDataDirectory, "raphcare-vendor-probe-log.txt"),
            ResolveDefaultMirrorTrailPath())
    {
    }

    /// <summary>Test seam.</summary>
    public FileVendorConnectStepProbe(string path, string? trailPath = null, string? mirrorTrailPath = null)
    {
        _path = path ?? throw new ArgumentNullException(nameof(path));
        _trailPath = string.IsNullOrWhiteSpace(trailPath)
            ? Path.Combine(
                Path.GetDirectoryName(path) ?? FileSystem.Current.AppDataDirectory,
                "raphcare-vendor-probe-log.txt")
            : trailPath;
        _mirrorTrailPath = string.IsNullOrWhiteSpace(mirrorTrailPath) ? null : mirrorTrailPath;
    }

    public string TrailFilePath => _trailPath;

    public string ShareableTrailPath => _mirrorTrailPath ?? _trailPath;

    public bool HasTrailLog
    {
        get
        {
            lock (_sync)
            {
                try
                {
                    var p = ShareableTrailPath;
                    return File.Exists(p) && new FileInfo(p).Length > 0;
                }
                catch
                {
                    return false;
                }
            }
        }
    }

    public void Mark(string stepCode)
    {
        if (string.IsNullOrWhiteSpace(stepCode))
            return;

        var step = stepCode.Trim();
        lock (_sync)
        {
            // Trail always records the step (including ClearsProbe markers) so a crash after
            // SCAN-STOP / INIT-3 still leaves a readable history on the phone.
            AppendTrailUnlocked(step);

            if (VendorConnectCrashProbeRules.ClearsProbe(step))
            {
                TryDeleteUnlocked();
                return;
            }

            WriteThroughUnlocked(step);
        }
    }

    public void Clear()
    {
        lock (_sync)
            TryDeleteUnlocked();
    }

    public string? PeekIncompleteStep()
    {
        lock (_sync)
        {
            if (!File.Exists(_path))
                return null;

            try
            {
                var step = File.ReadAllText(_path).Trim();
                return string.IsNullOrWhiteSpace(step) ? null : step;
            }
            catch
            {
                return null;
            }
        }
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

    private void AppendTrailUnlocked(string step)
    {
        var line = DateTimeOffset.UtcNow.ToString("o") + " " + step + Environment.NewLine;
        AppendWriteThrough(_trailPath, line);
        if (_mirrorTrailPath is not null
            && !string.Equals(_mirrorTrailPath, _trailPath, StringComparison.OrdinalIgnoreCase))
        {
            AppendWriteThrough(_mirrorTrailPath, line);
        }
    }

    private static void AppendWriteThrough(string path, string line)
    {
        try
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            using var fs = new FileStream(
                path,
                FileMode.Append,
                FileAccess.Write,
                FileShare.Read,
                bufferSize: 256,
                FileOptions.WriteThrough);
            using var writer = new StreamWriter(fs);
            writer.Write(line);
            writer.Flush();
            fs.Flush(flushToDisk: true);
        }
        catch
        {
            // Best-effort trail; never fail Mark because of mirror I/O.
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

    private static string? ResolveDefaultMirrorTrailPath()
    {
#if ANDROID
        try
        {
            return Platforms.Android.HBand.AndroidVendorProbeLogLocator.GetExternalFilesTrailPath();
        }
        catch
        {
            return null;
        }
#else
        return null;
#endif
    }
}
