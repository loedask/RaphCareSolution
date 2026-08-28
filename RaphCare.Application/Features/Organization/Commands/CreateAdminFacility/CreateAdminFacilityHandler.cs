using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminFacility;

public sealed class CreateAdminFacilityHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IRepository<Clinic> clinicRepository,
    IRepository<Facility> facilityRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateAdminFacilityCommand, FacilityListItemDto?>
{
    public async Task<FacilityListItemDto?> Handle(
        CreateAdminFacilityCommand request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.HasClinicAccessAsync(
                currentUserService, clinicStaffMembershipService, request.ClinicId, cancellationToken)
            .ConfigureAwait(false))
            return null;

        var clinic = await clinicRepository.GetByIdAsync(request.ClinicId, cancellationToken).ConfigureAwait(false);
        if (clinic is null)
            return null;

        var facility = new Facility
        {
            ClinicId = clinic.Id,
            Name = request.Name.Trim(),
            Address = request.Address.Trim(),
            City = request.City.Trim(),
            Country = string.IsNullOrWhiteSpace(request.Country) ? clinic.Country : request.Country.Trim(),
            IsVirtual = request.IsVirtual
        };

        await facilityRepository.AddAsync(facility, cancellationToken).ConfigureAwait(false);
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
