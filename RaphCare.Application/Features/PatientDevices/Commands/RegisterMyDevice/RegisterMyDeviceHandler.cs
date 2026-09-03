using FluentValidation.Results;
using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.PatientDevices.DTOs;
using RaphCare.Domain.Devices;
using ValidationException = RaphCare.Application.Common.Exceptions.ValidationException;

namespace RaphCare.Application.Features.PatientDevices.Commands.RegisterMyDevice;

/// <summary>
/// Confirms a fleet wearable already assigned to the current patient (claim / activate in the app).
/// Does not create inventory or assignments; platform ops must register and assign first.
/// </summary>
public sealed class RegisterMyDeviceHandler : IRequestHandler<RegisterMyDeviceCommand, RegisterMyDeviceResponseDto>
{
    private readonly IRepository<Device> _devices;
    private readonly IRepository<DeviceAssignment> _assignments;
    private readonly ICurrentUserService _currentUser;
    private readonly IPatientClinicAccessService _patientClinics;

    public RegisterMyDeviceHandler(
        IRepository<Device> devices,
        IRepository<DeviceAssignment> assignments,
        ICurrentUserService currentUser,
        IPatientClinicAccessService patientClinics)
    {
        _devices = devices;
        _assignments = assignments;
        _currentUser = currentUser;
        _patientClinics = patientClinics;
    }

    public async Task<RegisterMyDeviceResponseDto> Handle(RegisterMyDeviceCommand request, CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required.");

        var serial = request.SerialNumber.Trim();

        var clinicIds = await _patientClinics.GetAccessibleClinicIdsAsync(patientId, cancellationToken).ConfigureAwait(false);
        if (clinicIds.Length == 0)
        {
            throw new ValidationException(
            [
                new ValidationFailure(nameof(RegisterMyDeviceCommand), "Register your care with a clinic before claiming a device.")
            ]);
        }

        var existingPaged = await _devices.SearchAsync(
            q => q.Where(d => d.SerialNumber == serial),
            1,
            1,
            false,
            cancellationToken).ConfigureAwait(false);

        var existing = existingPaged.Items.Count > 0 ? existingPaged.Items[0] : null;
        if (existing is null)
        {
            throw new ValidationException(
            [
                new ValidationFailure(
                    nameof(RegisterMyDeviceCommand.SerialNumber),
                    "This serial is not in the RaphCare fleet. Ask your care programme to assign a watch first.")
            ]);
        }

        if (!existing.IsActive)
        {
            throw new ValidationException(
            [
                new ValidationFailure(nameof(RegisterMyDeviceCommand.SerialNumber), "This device is not active.")
            ]);
        }

        if (!clinicIds.Contains(existing.ClinicId))
        {
            throw new ValidationException(
            [
                new ValidationFailure(
                    nameof(RegisterMyDeviceCommand.SerialNumber),
                    "This device belongs to another clinic programme.")
            ]);
        }

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
            throw new ValidationException(
            [
                new ValidationFailure(nameof(RegisterMyDeviceCommand.SerialNumber), "This device serial is already assigned to another patient.")
            ]);
        }

        throw new ValidationException(
        [
            new ValidationFailure(
                nameof(RegisterMyDeviceCommand.SerialNumber),
                "This watch is not assigned to you yet. Ask RaphCare or your clinic to assign it first.")
        ]);
    }
}
