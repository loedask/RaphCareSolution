using FluentValidation.Results;
using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.PatientDevices.DTOs;
using RaphCare.Domain.Devices;
using RaphCare.Domain.Organization;
using ValidationException = RaphCare.Application.Common.Exceptions.ValidationException;

namespace RaphCare.Application.Features.PatientDevices.Commands.RegisterMyDevice;

/// <summary>
/// Confirms a fleet wearable for the current patient.
/// Hospital programmes require a prior staff assignment.
/// Direct / self-claim clinics allow packaging serial claim of in-stock devices (creates the assignment).
/// </summary>
public sealed class RegisterMyDeviceHandler : IRequestHandler<RegisterMyDeviceCommand, RegisterMyDeviceResponseDto>
{
    private readonly IRepository<Device> _devices;
    private readonly IRepository<DeviceAssignment> _assignments;
    private readonly IRepository<Clinic> _clinics;
    private readonly ICurrentUserService _currentUser;
    private readonly IPatientClinicAccessService _patientClinics;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _clock;

    public RegisterMyDeviceHandler(
        IRepository<Device> devices,
        IRepository<DeviceAssignment> assignments,
        IRepository<Clinic> clinics,
        ICurrentUserService currentUser,
        IPatientClinicAccessService patientClinics,
        IUnitOfWork unitOfWork,
        IDateTimeProvider clock)
    {
        _devices = devices;
        _assignments = assignments;
        _clinics = clinics;
        _currentUser = currentUser;
        _patientClinics = patientClinics;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<RegisterMyDeviceResponseDto> Handle(RegisterMyDeviceCommand request, CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required.");

        var serial = request.SerialNumber.Trim();

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

        var clinic = await _clinics.GetByIdAsync(existing.ClinicId, cancellationToken).ConfigureAwait(false)
            ?? throw new ValidationException(
            [
                new ValidationFailure(nameof(RegisterMyDeviceCommand.SerialNumber), "This device programme is not available.")
            ]);

        var clinicIds = await _patientClinics.GetAccessibleClinicIdsAsync(patientId, cancellationToken).ConfigureAwait(false);
        var hasClinicAccess = clinicIds.Contains(existing.ClinicId);
        var canSelfClaim = clinic.AllowPatientDeviceSelfClaim
            && !existing.IsAssigned
            && string.Equals(existing.Status, "InStock", StringComparison.OrdinalIgnoreCase);

        if (!hasClinicAccess)
        {
            if (!canSelfClaim)
            {
                if (clinicIds.Length == 0)
                {
                    throw new ValidationException(
                    [
                        new ValidationFailure(nameof(RegisterMyDeviceCommand), "Register your care with a clinic before claiming a device.")
                    ]);
                }

                throw new ValidationException(
                [
                    new ValidationFailure(
                        nameof(RegisterMyDeviceCommand.SerialNumber),
                        "This device belongs to another clinic programme.")
                ]);
            }

            await _patientClinics.GrantManualAccessAsync(
                    patientId,
                    existing.ClinicId,
                    "Packaging self-claim for Direct care package",
                    cancellationToken)
                .ConfigureAwait(false);
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

        if (!canSelfClaim && !clinic.AllowPatientDeviceSelfClaim)
        {
            throw new ValidationException(
            [
                new ValidationFailure(
                    nameof(RegisterMyDeviceCommand.SerialNumber),
                    "This watch is not assigned to you yet. Ask RaphCare or your clinic to assign it first.")
            ]);
        }

        if (!canSelfClaim)
        {
            throw new ValidationException(
            [
                new ValidationFailure(
                    nameof(RegisterMyDeviceCommand.SerialNumber),
                    "This watch is not available for packaging claim. Ask RaphCare or your clinic to assign it first.")
            ]);
        }

        await _assignments.AddAsync(
            new DeviceAssignment
            {
                DeviceId = existing.Id,
                PatientId = patientId,
                AssignedAt = _clock.UtcNow,
                IsActive = true
            },
            cancellationToken).ConfigureAwait(false);

        existing.IsAssigned = true;
        existing.Status = "Assigned";
        existing.ActivatedAt ??= _clock.UtcNow;
        await _devices.UpdateAsync(existing, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new RegisterMyDeviceResponseDto { DeviceId = existing.Id };
    }
}
