using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Clinical;

namespace RaphCare.Application.Features.Organization.Commands.UpsertAdminClinicConsentTemplate;

public sealed class UpsertAdminClinicConsentTemplateHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IUserRoleAssignmentService roleAssignmentService,
    IRepository<ClinicConsentTemplate> templateRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpsertAdminClinicConsentTemplateCommand, AdminClinicConsentTemplateDto?>
{
    public async Task<AdminClinicConsentTemplateDto?> Handle(
        UpsertAdminClinicConsentTemplateCommand request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.IsClinicAdministratorAsync(
                currentUserService,
                clinicStaffMembershipService,
                roleAssignmentService,
                request.ClinicId,
                cancellationToken)
            .ConfigureAwait(false))
            throw new ForbiddenAccessException("Only hospital administrators can edit the consent template.");

        ClinicConsentTemplate template;
        var isNew = false;

        if (request.TemplateId is { } templateId)
        {
            var existing = await templateRepository.GetByIdAsync(templateId, cancellationToken).ConfigureAwait(false);
            if (existing is null || existing.ClinicId != request.ClinicId)
                return null;
            template = existing;
        }
        else
        {
            var activePage = await templateRepository.SearchAsync(
                q => q.Where(t => t.ClinicId == request.ClinicId && t.IsActive),
                1,
                1,
                cancellationToken: cancellationToken).ConfigureAwait(false);
            if (activePage.Items.Count > 0)
            {
                template = activePage.Items[0];
            }
            else
            {
                var anyPage = await templateRepository.SearchAsync(
                    q => q.Where(t => t.ClinicId == request.ClinicId).OrderByDescending(t => t.CreatedAt),
                    1,
                    1,
                    applyDefaultIdOrdering: false,
                    cancellationToken: cancellationToken).ConfigureAwait(false);
                if (anyPage.Items.Count > 0)
                {
                    template = anyPage.Items[0];
                }
                else
                {
                    template = new ClinicConsentTemplate { ClinicId = request.ClinicId };
                    isNew = true;
                }
            }
        }

        template.Title = request.Title.Trim();
        template.Body = request.Body.Trim();
        template.IsActive = request.IsActive;

        if (isNew)
            await templateRepository.AddAsync(template, cancellationToken).ConfigureAwait(false);
        else
            await templateRepository.UpdateAsync(template, cancellationToken).ConfigureAwait(false);

        if (template.IsActive)
        {
            var others = await templateRepository.SearchAsync(
                q => q.Where(t => t.ClinicId == request.ClinicId && t.Id != template.Id && t.IsActive),
                1,
                100,
                cancellationToken: cancellationToken).ConfigureAwait(false);
            foreach (var other in others.Items)
            {
                other.IsActive = false;
                await templateRepository.UpdateAsync(other, cancellationToken).ConfigureAwait(false);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

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
