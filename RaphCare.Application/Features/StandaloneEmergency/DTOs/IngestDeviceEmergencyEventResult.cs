namespace RaphCare.Application.Features.StandaloneEmergency.DTOs;

public sealed class IngestDeviceEmergencyEventResult
{
    public Guid Id { get; set; }
    public bool WasDuplicate { get; set; }
}
