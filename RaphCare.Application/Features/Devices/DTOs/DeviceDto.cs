using RaphCare.Application.Common.DTOs;

namespace RaphCare.Application.Features.Devices.DTOs;

public class DeviceDto : BaseDto
{
    public Guid ClinicId { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsAssigned { get; set; }
    public string Status { get; set; } = string.Empty;
}

