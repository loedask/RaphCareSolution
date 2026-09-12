using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Clinical;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinicConsentTemplate;

public sealed class GetAdminClinicConsentTemplateHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IRepository<ClinicConsentTemplate> templateRepository)
    : IRequestHandler<GetAdminClinicConsentTemplateQuery, AdminClinicConsentTemplateDto?>
{
    public async Task<AdminClinicConsentTemplateDto?> Handle(
        GetAdminClinicConsentTemplateQuery request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.HasClinicAccessAsync(
                currentUserService,
                clinicStaffMembershipService,
                request.ClinicId,
                cancellationToken)
            .ConfigureAwait(false))
            return null;

        var activePage = await templateRepository.SearchAsync(
            q => q.Where(t => t.ClinicId == request.ClinicId && t.IsActive),
            1,
            1,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        ClinicConsentTemplate? template = activePage.Items.Count > 0 ? activePage.Items[0] : null;
        if (template is null)
        {
            var anyPage = await templateRepository.SearchAsync(
                q => q.Where(t => t.ClinicId == request.ClinicId).OrderByDescending(t => t.CreatedAt),
                1,
                1,
                applyDefaultIdOrdering: false,
                cancellationToken: cancellationToken).ConfigureAwait(false);
            template = anyPage.Items.Count > 0 ? anyPage.Items[0] : null;
        }

        if (template is null)
            return null;

        return new AdminClinicConsentTemplateDto
        {
            Id = template.Id,
            ClinicId = template.ClinicId,
            Title = template.Title,
            Body = template.Body,
            IsActive = template.IsActive
        };
    }
}
