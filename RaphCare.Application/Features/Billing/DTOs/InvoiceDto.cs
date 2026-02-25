using RaphCare.Application.Common.DTOs;

namespace RaphCare.Application.Features.Billing.DTOs;

public class InvoiceDto : BaseDto
{
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    public Guid? VisitId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public DateTime? PaidAt { get; set; }
}
