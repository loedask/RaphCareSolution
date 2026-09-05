namespace RaphCare.Client.Models.Fleet;

public sealed class FleetDeviceListItem
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string? BluetoothMacAddress { get; set; }
    public DateTime? ActivatedAt { get; set; }
    public bool IsActive { get; set; }
    public bool IsAssigned { get; set; }
    public string Status { get; set; } = string.Empty;
}

public sealed class PagedFleetDevices
{
    public IReadOnlyList<FleetDeviceListItem> Items { get; set; } = Array.Empty<FleetDeviceListItem>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
