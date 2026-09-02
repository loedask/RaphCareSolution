using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.DraftAdminClinicDischargeSummary;

public sealed class DraftAdminClinicDischargeSummaryCommand : IRequest<AdminClinicDischargeSummaryDraftDto?>
{
    public Guid ClinicId { get; set; }
    public Guid AdmissionId { get; set; }
}
