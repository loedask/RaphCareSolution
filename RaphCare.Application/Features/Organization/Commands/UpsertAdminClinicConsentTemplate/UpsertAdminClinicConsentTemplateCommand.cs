using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.UpsertAdminClinicConsentTemplate;

public sealed class UpsertAdminClinicConsentTemplateCommand : IRequest<AdminClinicConsentTemplateDto?>
{
    public Guid ClinicId { get; init; }
    public Guid? TemplateId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public bool IsActive { get; init; } = true;
}
