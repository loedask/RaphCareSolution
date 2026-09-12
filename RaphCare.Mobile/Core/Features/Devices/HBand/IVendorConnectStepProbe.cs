namespace RaphCare.Mobile.Core.Features.Devices.HBand;

/// <summary>
/// Persists the last vendor Connect step so a native process kill can be reported on relaunch.
/// Also keeps an append-only trail the patient can Share or copy from the phone without adb.
/// </summary>
public interface IVendorConnectStepProbe
{
    /// <summary>Writes <paramref name="stepCode"/> to durable storage before the next risky JNI call.</summary>
    void Mark(string stepCode);

    /// <summary>Clears any stored single-step breadcrumb (successful Connect or user dismissed).</summary>
    void Clear();

    /// <summary>
    /// If the last mark looks like a crash mid-Connect, returns that step and clears the single-step file.
    /// Does not erase the append-only trail.
    /// </summary>
    string? TryConsumeIncompleteStep();

    /// <summary>Read the single-step breadcrumb without deleting it (self-test / diagnostics).</summary>
    string? PeekIncompleteStep();

    /// <summary>Path to the append-only probe trail (AppData). Prefer <see cref="ShareableTrailPath"/> when set.</summary>
    string TrailFilePath { get; }

    /// <summary>
    /// Best path for the patient to find or Share the trail (often under Android/data/.../files).
    /// </summary>
    string ShareableTrailPath { get; }

    /// <summary>True when the trail file exists and has content.</summary>
    bool HasTrailLog { get; }
}
