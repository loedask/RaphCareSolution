using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.CompleteAdminClinicVisit;

public sealed class CompleteAdminClinicVisitCommand : IRequest<AdminClinicVisitDetailDto?>
{
    public Guid ClinicId { get; set; }
    public Guid VisitId { get; set; }
    public string? Summary { get; set; }
    public decimal? BillAmount { get; set; }
    public string? BillDescription { get; set; }
    public bool MarkPaid { get; set; }
    public string? Currency { get; set; }
}
