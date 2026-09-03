using MediatR;
using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinics;

public sealed class GetAdminClinicsHandler(
    IRepository<Clinic> clinicRepository,
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IUserRoleAssignmentService roleAssignmentService)
    : IRequestHandler<GetAdminClinicsQuery, PagedResult<ClinicListItemDto>>
{
    public async Task<PagedResult<ClinicListItemDto>> Handle(
        GetAdminClinicsQuery request,
        CancellationToken cancellationToken)
    {
        if (currentUserService.CurrentUserId is not { } userId)
        {
            return EmptyPage(request);
        }

        var isPlatformAdmin = await AdminClinicAuthorization
            .IsPlatformAdministratorAsync(currentUserService, roleAssignmentService, cancellationToken)
            .ConfigureAwait(false);

        var membershipClinicIds = await clinicStaffMembershipService
            .GetClinicIdsForUserAsync(userId, cancellationToken)
            .ConfigureAwait(false);

        // Platform admins with no clinic membership (Ops) see every active hospital for fleet work.
        // Hospital admins who also have the Administrator role still only see membership hospitals.
        PagedResult<Clinic> paged;
        if (isPlatformAdmin && membershipClinicIds.Count == 0)
        {
            paged = await clinicRepository.SearchAsync(
                queryShaper: q => q
                    .Include(c => c.Facilities)
                    .Where(c => c.IsActive)
                    .OrderByDescending(c => c.CreatedAt),
                pageNumber: request.PageNumber,
                pageSize: request.PageSize,
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        else
        {
            if (membershipClinicIds.Count == 0)
                return EmptyPage(request);

            paged = await clinicRepository.SearchAsync(
                queryShaper: q => q
                    .Include(c => c.Facilities)
                    .Where(c => membershipClinicIds.Contains(c.Id))
                    .OrderByDescending(c => c.CreatedAt),
                pageNumber: request.PageNumber,
                pageSize: request.PageSize,
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        var items = paged.Items
            .Select(c => new ClinicListItemDto
            {
                Id = c.Id,
                Name = c.Name,
                RegistrationNumber = c.RegistrationNumber,
                ReferenceCode = c.ReferenceCode,
                Country = c.Country,
                TimeZone = c.TimeZone,
                IsActive = c.IsActive,
                FacilityCount = c.Facilities.Count,
                CreatedAt = c.CreatedAt
            })
            .ToList();

        return new PagedResult<ClinicListItemDto>
        {
            Items = items,
            PageNumber = paged.PageNumber,
            PageSize = paged.PageSize,
            TotalCount = paged.TotalCount
        };
    }

    private static PagedResult<ClinicListItemDto> EmptyPage(GetAdminClinicsQuery request) =>
        new()
        {
            Items = [],
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = 0
        };
}
