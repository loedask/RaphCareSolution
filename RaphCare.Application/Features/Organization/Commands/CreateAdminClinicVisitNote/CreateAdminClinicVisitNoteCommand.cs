using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicVisitNote;

public sealed class CreateAdminClinicVisitNoteCommand : IRequest<AdminClinicVisitNoteDto?>
{
    public Guid ClinicId { get; set; }
    public Guid VisitId { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string? Category { get; set; }
}
