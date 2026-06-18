using MediatR;
using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Organization.Commands.RegisterClinic;

public sealed class RegisterClinicHandler(
    IRepository<Clinic> clinicRepository,
    IUnitOfWork unitOfWork,
    IUniqueConstraintViolationDetector uniqueConstraintDetector) : IRequestHandler<RegisterClinicCommand, RegisterClinicResultDto>
{
    public async Task<RegisterClinicResultDto> Handle(RegisterClinicCommand request, CancellationToken cancellationToken)
    {
        var clinic = new Clinic
        {
            Name = request.Name.Trim(),
            RegistrationNumber = request.RegistrationNumber.Trim(),
            Country = request.Country.Trim(),
            TimeZone = request.TimeZone.Trim(),
            IsActive = true
        };

        Facility? facility = null;
        if (!string.IsNullOrWhiteSpace(request.FacilityName))
        {
            facility = new Facility
            {
                Name = request.FacilityName.Trim(),
                Address = request.FacilityAddress?.Trim() ?? string.Empty,
                City = request.FacilityCity?.Trim() ?? string.Empty,
                Country = request.Country.Trim(),
                IsVirtual = request.IsVirtualFacility
            };
            clinic.Facilities.Add(facility);
        }

        try
        {
            await clinicRepository.AddAsync(clinic, cancellationToken).ConfigureAwait(false);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateException ex) when (uniqueConstraintDetector.IsUniqueConstraintViolation(ex))
        {
            throw new InvalidOperationException(
                "A clinic with this registration number already exists.",
                ex);
        }

        return new RegisterClinicResultDto
        {
            ClinicId = clinic.Id,
            Name = clinic.Name,
            PrimaryFacilityId = facility?.Id,
            PrimaryFacilityName = facility?.Name
        };
    }
}
