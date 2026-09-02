using RaphCare.Domain.Common;

namespace RaphCare.Domain.Clinical;

/// <summary>Ward note or vitals recorded against an inpatient stay.</summary>
public class InpatientObservation : BaseEntity
{
    public Guid ClinicId { get; set; }
    public Guid AdmissionId { get; set; }
    public DateTime RecordedAt { get; set; }
    public Guid? RecordedByApplicationUserId { get; set; }
    public string Note { get; set; } = string.Empty;
    public decimal? HeartRate { get; set; }
    public decimal? TemperatureCelsius { get; set; }
    public decimal? OxygenSaturation { get; set; }
    public decimal? SystolicBp { get; set; }
    public decimal? DiastolicBp { get; set; }

    public InpatientAdmission Admission { get; set; } = null!;
}
