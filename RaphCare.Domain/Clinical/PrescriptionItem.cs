using RaphCare.Domain.Common;

namespace RaphCare.Domain.Clinical;

/// <summary>
/// Line item on a prescription.
/// </summary>
public class PrescriptionItem : BaseEntity
{
    public Guid PrescriptionId { get; set; }
    public string MedicationName { get; set; } = string.Empty;
    public string? Dosage { get; set; }
    public string? Frequency { get; set; }
    public int DurationDays { get; set; }

    public Prescription Prescription { get; set; } = null!;
}
