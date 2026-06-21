using FluentValidation.Results;
using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.PatientDevices.DTOs;
using RaphCare.Domain.Devices;

namespace RaphCare.Application.Features.PatientDevices.Commands.RegisterMyDevice;

public sealed class RegisterMyDeviceHandler : IRequestHandler<RegisterMyDeviceCommand, RegisterMyDeviceResponseDto>
{
    private readonly IRepository<Device> _devices;
    private readonly IRepository<DeviceAssignment> _assignments;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IPatientClinicAccessService _patientClinics;
    private readonly IDateTimeProvider _clock;

    public RegisterMyDeviceHandler(
        IRepository<Device> devices,
        IRepository<DeviceAssignment> assignments,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IPatientClinicAccessService patientClinics,
        IDateTimeProvider clock)
    {
        _devices = devices;
        _assignments = assignments;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _patientClinics = patientClinics;
        _clock = clock;
    }

    public async Task<RegisterMyDeviceResponseDto> Handle(RegisterMyDeviceCommand request, CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required.");

        var serial = request.SerialNumber.Trim();
        var model = request.ModelSku.Trim();

        var clinicIds = await _patientClinics.GetAccessibleClinicIdsAsync(patientId, cancellationToken).ConfigureAwait(false);
        if (clinicIds.Length == 0)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure(nameof(RegisterMyDeviceCommand), "Register your care with a clinic (visit or enrollment) before adding a device.")
            });
        }

        var clinicId = clinicIds.OrderBy(x => x).First();

        var existingPaged = await _devices.SearchAsync(
            q => q.Where(d => d.SerialNumber == serial),
            1,
            1,
            false,
            cancellationToken).ConfigureAwait(false);

        var existing = existingPaged.Items.Count > 0 ? existingPaged.Items[0] : null;
        if (existing != null)
        {
            var myAssignment = await _assignments.SearchAsync(
                q => q.Where(a => a.DeviceId == existing.Id && a.PatientId == patientId && a.IsActive),
                1,
                1,
                true,
                cancellationToken).ConfigureAwait(false);

            if (myAssignment.Items.Count > 0)
                return new RegisterMyDeviceResponseDto { DeviceId = existing.Id };

            var otherActive = await _assignments.SearchAsync(
                q => q.Where(a => a.DeviceId == existing.Id && a.PatientId != patientId && a.IsActive),
                1,
                1,
                true,
                cancellationToken).ConfigureAwait(false);

            if (otherActive.Items.Count > 0)
            {
                throw new ValidationException(new[]
                {
                    new ValidationFailure(nameof(RegisterMyDeviceCommand.SerialNumber), "This device serial is already assigned to another patient.")
                });
            }

            var assignment = new DeviceAssignment
            {
                DeviceId = existing.Id,
                PatientId = patientId,
                AssignedAt = _clock.UtcNow,
                IsActive = true
            };
            existing.IsAssigned = true;
            await _assignments.AddAsync(assignment, cancellationToken).ConfigureAwait(false);
            await _devices.UpdateAsync(existing, cancellationToken).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return new RegisterMyDeviceResponseDto { DeviceId = existing.Id };
        }

        var device = new Device
        {
            ClinicId = clinicId,
            SerialNumber = serial,
            Model = model,
            DeviceTypeId = KnownDeviceCatalogIds.WearableBleDeviceTypeId,
            DeviceManufacturerId = KnownDeviceCatalogIds.GenericOemManufacturerId,
            IsActive = true,
            IsAssigned = true,
            Status = "Active",
            ActivatedAt = _clock.UtcNow
        };

        await _devices.AddAsync(device, cancellationToken).ConfigureAwait(false);

        await _assignments.AddAsync(
            new DeviceAssignment
            {
                DeviceId = device.Id,
                PatientId = patientId,
                AssignedAt = _clock.UtcNow,
                IsActive = true
            },
            cancellationToken).ConfigureAwait(false);

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new RegisterMyDeviceResponseDto { DeviceId = device.Id };
    }
}
