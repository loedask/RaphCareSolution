namespace RaphCare.Mobile.Core.Features.Settings.Models;

/// <summary>One hospital from the patient membership / care-history list on Active clinic.</summary>
public sealed class LinkedClinicDisplayItem
{
    public Guid ClinicId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ReferenceCode { get; init; } = string.Empty;
    public string AccessLabel { get; init; } = string.Empty;
    public bool IsActiveOnPhone { get; init; }
}
