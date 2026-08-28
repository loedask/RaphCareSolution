using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.UpsertAdminClinicVisitSoapNote;

public sealed class UpsertAdminClinicVisitSoapNoteCommand : IRequest<AdminClinicVisitSoapNoteDto?>
{
    public Guid ClinicId { get; set; }
    public Guid VisitId { get; set; }
    public string? Subjective { get; set; }
    public string? Objective { get; set; }
    public string? Assessment { get; set; }
    public string? Plan { get; set; }
}
