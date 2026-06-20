using MediatR;
using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Organization.Queries.GetAdminClinicById;

public sealed class GetAdminClinicByIdHandler(
    IRepository<Clinic> clinicRepository,
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService)
    : IRequestHandler<GetAdminClinicByIdQuery, ClinicDetailDto?>
{
    public async Task<ClinicDetailDto?> Handle(GetAdminClinicByIdQuery request, CancellationToken cancellationToken)
    {
        if (currentUserService.CurrentUserId is not { } userId)
            return null;

        if (!await clinicStaffMembershipService.HasMembershipAsync(userId, request.ClinicId, cancellationToken).ConfigureAwait(false))
            return null;

        var clinic = await clinicRepository.SearchAsync(
            queryShaper: q => q
                .Include(c => c.Facilities)
                .Where(c => c.Id == request.ClinicId),
            pageNumber: 1,
            pageSize: 1,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        var entity = clinic.Items.FirstOrDefault();
        if (entity is null)
            return null;

        return new ClinicDetailDto
        {
            Id = entity.Id,
            Name = entity.Name,
            RegistrationNumber = entity.RegistrationNumber,
            Country = entity.Country,
            TimeZone = entity.TimeZone,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            Facilities = entity.Facilities
                .OrderBy(f => f.Name)
                .Select(f => new FacilityListItemDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    Address = f.Address,
                    City = f.City,
                    Country = f.Country,
                    IsVirtual = f.IsVirtual
                })
                .ToList()
        };
    }
}
