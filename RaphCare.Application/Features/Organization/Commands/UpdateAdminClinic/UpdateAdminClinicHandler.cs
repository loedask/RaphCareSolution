using MediatR;
using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Organization.Commands.UpdateAdminClinic;

public sealed class UpdateAdminClinicHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IRepository<Clinic> clinicRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateAdminClinicCommand, ClinicDetailDto?>
{
    public async Task<ClinicDetailDto?> Handle(UpdateAdminClinicCommand request, CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.HasClinicAccessAsync(
                currentUserService, clinicStaffMembershipService, request.ClinicId, cancellationToken)
            .ConfigureAwait(false))
            return null;

        var clinic = await clinicRepository.GetByIdAsync(request.ClinicId, cancellationToken).ConfigureAwait(false);
        if (clinic is null || clinic.IsDeleted)
            return null;

        clinic.Name = request.Name.Trim();
        clinic.Country = request.Country.Trim();
        clinic.TimeZone = request.TimeZone.Trim();

        await clinicRepository.UpdateAsync(clinic, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var page = await clinicRepository.SearchAsync(
            queryShaper: q => q.Include(c => c.Facilities).Where(c => c.Id == clinic.Id),
            pageNumber: 1,
            pageSize: 1,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        var updated = page.Items.FirstOrDefault();
        if (updated is null)
            return null;

        return new ClinicDetailDto
        {
            Id = updated.Id,
            Name = updated.Name,
            RegistrationNumber = updated.RegistrationNumber,
            Country = updated.Country,
            TimeZone = updated.TimeZone,
            IsActive = updated.IsActive,
            CreatedAt = updated.CreatedAt,
            Facilities = updated.Facilities
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
