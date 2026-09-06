namespace RaphCare.Client.Models.Clinics;

public sealed class PatientLinkedClinicViewModel
{
    public Guid ClinicId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ReferenceCode { get; set; } = string.Empty;
    public string AccessKind { get; set; } = string.Empty;
    public DateTime? GrantedAt { get; set; }
}
