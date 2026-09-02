using RaphCare.Domain.Common;
using RaphCare.Domain.Organization;
using RaphCare.Domain.Patients;

namespace RaphCare.Domain.Clinical;

/// <summary>Inpatient stay: patient assigned to a bed until discharge.</summary>
public class InpatientAdmission : BaseEntity
{
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    public Guid BedId { get; set; }
    public DateTime AdmittedAt { get; set; }
    public DateTime? DischargedAt { get; set; }
    public string Status { get; set; } = "Admitted";
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public string? DischargeSummary { get; set; }
    public Guid? InvoiceId { get; set; }
    public Guid? AdmittedByApplicationUserId { get; set; }

    public Clinic Clinic { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public Bed Bed { get; set; } = null!;

    /// <summary>Billable nights: at least one, otherwise whole days rounded up.</summary>
    public int CountChargeableBedNights(DateTime dischargedAtUtc)
    {
        var span = dischargedAtUtc - AdmittedAt;
        if (span <= TimeSpan.Zero)
            return 1;
        return Math.Max(1, (int)Math.Ceiling(span.TotalDays));
    }
}
