namespace RaphCare.Portal.Helpers;

/// <summary>
/// One step in the hospital-deck path under the masthead.
/// </summary>
/// <param name="Label">Visible text.</param>
/// <param name="Href">Link target; null means the current step.</param>
public readonly record struct HospitalDeckCrumb(string Label, string? Href = null);
