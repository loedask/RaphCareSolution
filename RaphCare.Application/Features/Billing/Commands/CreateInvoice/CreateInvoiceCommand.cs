using MediatR;

namespace RaphCare.Application.Features.Billing.Commands.CreateInvoice;

public class CreateInvoiceCommand : IRequest<Guid>
{
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    public Guid? VisitId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "ZAR";
    public DateTime DueDate { get; set; }
}
