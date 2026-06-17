namespace RaphCare.Client.Models.Integrations;

public sealed class IngestDeviceEmergencyEventResultViewModel
{
    public Guid Id { get; set; }
    public bool WasDuplicate { get; set; }
}
