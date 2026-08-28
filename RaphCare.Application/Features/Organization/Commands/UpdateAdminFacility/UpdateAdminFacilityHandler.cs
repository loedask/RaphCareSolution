using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Organization.Commands.UpdateAdminFacility;

public sealed class UpdateAdminFacilityHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IRepository<Facility> facilityRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateAdminFacilityCommand, FacilityListItemDto?>
{
    public async Task<FacilityListItemDto?> Handle(
        UpdateAdminFacilityCommand request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.HasClinicAccessAsync(
                currentUserService, clinicStaffMembershipService, request.ClinicId, cancellationToken)
            .ConfigureAwait(false))
            return null;

        var facility = await facilityRepository.GetByIdAsync(request.FacilityId, cancellationToken).ConfigureAwait(false);
        if (facility is null || facility.ClinicId != request.ClinicId)
            return null;

        facility.Name = request.Name.Trim();
        facility.Address = request.Address.Trim();
        facility.City = request.City.Trim();
        facility.Country = request.Country.Trim();
        facility.IsVirtual = request.IsVirtual;

        await facilityRepository.UpdateAsync(facility, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new FacilityListItemDto
        {
            Id = facility.Id,
            Name = facility.Name,
            Address = facility.Address,
            City = facility.City,
            Country = facility.Country,
            IsVirtual = facility.IsVirtual
        };
    }
}
