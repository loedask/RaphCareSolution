namespace RaphCare.Mobile.Core.Features.Settings.Models;

/// <summary>Device-local emergency contact (mirrors concept ICE list).</summary>
public sealed class StoredEmergencyContact
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}
