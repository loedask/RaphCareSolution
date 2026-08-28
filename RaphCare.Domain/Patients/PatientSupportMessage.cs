using RaphCare.Domain.Common;

namespace RaphCare.Domain.Patients;

/// <summary>Patient-submitted help desk message from the mobile app.</summary>
public class PatientSupportMessage : BaseEntity
{
    public Guid PatientId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? PatientEmail { get; set; }
    public Patient Patient { get; set; } = null!;
}
