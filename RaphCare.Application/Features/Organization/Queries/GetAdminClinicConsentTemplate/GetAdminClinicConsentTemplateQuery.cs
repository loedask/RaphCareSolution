using MediatR;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinicConsentTemplate;

public sealed class GetAdminClinicConsentTemplateQuery : IRequest<AdminClinicConsentTemplateDto?>
{
    public Guid ClinicId { get; init; }
}
