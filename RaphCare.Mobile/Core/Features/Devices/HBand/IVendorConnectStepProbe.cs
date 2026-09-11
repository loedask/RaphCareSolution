namespace RaphCare.Mobile.Core.Features.Devices.HBand;

/// <summary>
/// Persists the last vendor Connect step so a native process kill can be reported on relaunch.
/// </summary>
public interface IVendorConnectStepProbe
{
    /// <summary>Writes <paramref name="stepCode"/> to durable storage before the next risky JNI call.</summary>
    void Mark(string stepCode);

    /// <summary>Clears any stored step (successful Connect or user dismissed).</summary>
    void Clear();

    /// <summary>
    /// If the last mark looks like a crash mid-Connect, returns that step and clears storage.
    /// </summary>
    string? TryConsumeIncompleteStep();
}
