using FluentValidation.Results;
using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Devices;
using RaphCare.Domain.Patients;
using ValidationException = RaphCare.Application.Common.Exceptions.ValidationException;

namespace RaphCare.Application.Features.Devices.Commands.AssignDeviceToPatient;

public sealed class AssignDeviceToPatientHandler : IRequestHandler<AssignDeviceToPatientCommand, Guid>
{
    private readonly IRepository<Device> _devices;
    private readonly IRepository<DeviceAssignment> _assignments;
    private readonly IRepository<Patient> _patients;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _clock;

    public AssignDeviceToPatientHandler(
        IRepository<Device> devices,
        IRepository<DeviceAssignment> assignments,
        IRepository<Patient> patients,
        IUnitOfWork unitOfWork,
        IDateTimeProvider clock)
    {
        _devices = devices;
        _assignments = assignments;
        _patients = patients;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<Guid> Handle(AssignDeviceToPatientCommand request, CancellationToken cancellationToken)
    {
        var device = await _devices.GetByIdAsync(request.DeviceId, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException(nameof(Device), request.DeviceId);

        var patient = await _patients.GetByIdAsync(request.PatientId, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException(nameof(Patient), request.PatientId);

        if (!device.IsActive)
        {
            throw new ValidationException(
            [
                new ValidationFailure(nameof(AssignDeviceToPatientCommand.DeviceId), "Device is not active.")
            ]);
        }

        var existingForDevice = await _assignments.SearchAsync(
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
                new ValidationFailure(nameof(AssignDeviceToPatientCommand.DeviceId), "Device is already assigned to another patient.")
            ]);
        }

        await _assignments.AddAsync(
            new DeviceAssignment
            {
                DeviceId = device.Id,
                PatientId = patient.Id,
                AssignedAt = _clock.UtcNow,
                IsActive = true
            },
            cancellationToken).ConfigureAwait(false);

        device.IsAssigned = true;
        device.Status = "Assigned";
        device.ActivatedAt ??= _clock.UtcNow;
        await _devices.UpdateAsync(device, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return device.Id;
    }
}
