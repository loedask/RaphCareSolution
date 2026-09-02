using RaphCare.Domain.Common;

namespace RaphCare.Domain.Billing;

/// <summary>
/// Billing invoice for a patient or visit.
/// </summary>
public class Invoice : BaseEntity
{
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    public Guid? VisitId { get; set; }
    public Guid? AdmissionId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "ZAR";
    public string Status { get; set; } = string.Empty;
    public string? PaymentMethod { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? PaidAt { get; set; }
}
