using FluentValidation.Results;
using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Devices;
using RaphCare.Domain.Patients;
using ValidationException = RaphCare.Application.Common.Exceptions.ValidationException;

namespace RaphCare.Application.Features.Organization.Commands.AssignAdminClinicDeviceToPatient;

public sealed class AssignAdminClinicDeviceToPatientHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IPatientClinicAccessService patientClinicAccessService,
    IRepository<Device> devices,
    IRepository<DeviceAssignment> assignments,
    IRepository<Patient> patients,
    IUnitOfWork unitOfWork,
    IDateTimeProvider clock)
    : IRequestHandler<AssignAdminClinicDeviceToPatientCommand, Guid?>
{
    public async Task<Guid?> Handle(AssignAdminClinicDeviceToPatientCommand request, CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.HasClinicAccessAsync(
                currentUserService, clinicStaffMembershipService, request.ClinicId, cancellationToken)
            .ConfigureAwait(false))
            return null;

        var device = await devices.GetByIdAsync(request.DeviceId, cancellationToken).ConfigureAwait(false);
        if (device is null || device.ClinicId != request.ClinicId)
            return null;

        var patient = await patients.GetByIdAsync(request.PatientId, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException(nameof(Patient), request.PatientId);

        if (!await patientClinicAccessService.HasClinicAccessAsync(patient.Id, request.ClinicId, cancellationToken)
                .ConfigureAwait(false))
        {
            throw new ValidationException(
            [
                new ValidationFailure(
                    nameof(AssignAdminClinicDeviceToPatientCommand.PatientId),
                    "That patient is not linked to this hospital yet.")
            ]);
        }

        if (!device.IsActive)
        {
            throw new ValidationException(
            [
                new ValidationFailure(nameof(AssignAdminClinicDeviceToPatientCommand.DeviceId), "Device is not active.")
            ]);
        }

        var existingForDevice = await assignments.SearchAsync(
            q => q.Where(a => a.DeviceId == device.Id && a.IsActive),
            1,
            1,
            true,
            cancellationToken).ConfigureAwait(false);

        if (existingForDevice.Items.Count > 0)
        {
            var current = existingForDevice.Items[0];
            if (current.PatientId == patient.Id)
                return device.Id;

            throw new ValidationException(
            [
                new ValidationFailure(
                    nameof(AssignAdminClinicDeviceToPatientCommand.DeviceId),
                    "Device is already assigned to another patient.")
            ]);
        }

        await assignments.AddAsync(
            new DeviceAssignment
            {
                DeviceId = device.Id,
                PatientId = patient.Id,
                AssignedAt = clock.UtcNow,
                IsActive = true
            },
            cancellationToken).ConfigureAwait(false);

        device.IsAssigned = true;
        device.Status = "Assigned";
        device.ActivatedAt ??= clock.UtcNow;
        await devices.UpdateAsync(device, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return device.Id;
    }
}
