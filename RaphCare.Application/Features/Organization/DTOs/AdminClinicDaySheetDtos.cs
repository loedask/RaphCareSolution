namespace RaphCare.Application.Features.Organization.DTOs;

public sealed class AdminClinicDaySheetItemDto
{
    public Guid InvoiceId { get; set; }
    public Guid VisitId { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "ZAR";
    public string Status { get; set; } = string.Empty;
    public DateTime? PaidAt { get; set; }
    public string Description { get; set; } = string.Empty;
}
