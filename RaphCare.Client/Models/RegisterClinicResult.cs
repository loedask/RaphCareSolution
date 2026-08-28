namespace RaphCare.Client.Models;

public sealed class RegisterClinicResult
{
    public Guid ClinicId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ReferenceCode { get; set; } = string.Empty;
    public Guid? PrimaryFacilityId { get; set; }
    public string? PrimaryFacilityName { get; set; }
}
