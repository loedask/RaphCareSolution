using FluentValidation.Results;
using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Devices;
using ValidationException = RaphCare.Application.Common.Exceptions.ValidationException;

namespace RaphCare.Application.Features.Devices.Commands.DeleteDevice;

public sealed class DeleteDeviceHandler : IRequestHandler<DeleteDeviceCommand>
{
    private readonly IRepository<Device> _devices;
    private readonly IRepository<DeviceAssignment> _assignments;
    private readonly IRepository<DeviceReading> _readings;
    private readonly IRepository<DeviceEmergencyEvent> _emergencies;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteDeviceHandler(
        IRepository<Device> devices,
        IRepository<DeviceAssignment> assignments,
        IRepository<DeviceReading> readings,
        IRepository<DeviceEmergencyEvent> emergencies,
        IUnitOfWork unitOfWork)
    {
        _devices = devices;
        _assignments = assignments;
        _readings = readings;
        _emergencies = emergencies;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteDeviceCommand request, CancellationToken cancellationToken)
    {
        var device = await _devices.GetByIdAsync(request.DeviceId, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException(nameof(Device), request.DeviceId);

        if (device.IsAssigned)
        {
            throw new ValidationException(
            [
                new ValidationFailure(
                    nameof(DeleteDeviceCommand.DeviceId),
                    "Revoke the patient assignment before deleting this device.")
            ]);
        }

        var activeAssignment = await _assignments.SearchAsync(
            q => q.Where(a => a.DeviceId == device.Id && a.IsActive),
            1,
            1,
            true,
            cancellationToken).ConfigureAwait(false);

        if (activeAssignment.Items.Count > 0)
        {
            throw new ValidationException(
            [
                new ValidationFailure(
                    nameof(DeleteDeviceCommand.DeviceId),
                    "Revoke the patient assignment before deleting this device.")
            ]);
        }

        var hasHistory = await HasRelatedHistoryAsync(device.Id, cancellationToken).ConfigureAwait(false);
        if (hasHistory)
        {
            if (!device.IsActive && string.Equals(device.Status, "Retired", StringComparison.Ordinal))
                return;

            device.IsActive = false;
            device.Status = "Retired";
            await _devices.UpdateAsync(device, cancellationToken).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        await _devices.DeleteAsync(device, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<bool> HasRelatedHistoryAsync(Guid deviceId, CancellationToken cancellationToken)
    {
        var assignments = await _assignments.SearchAsync(
            q => q.Where(a => a.DeviceId == deviceId),
            1,
            1,
            true,
            cancellationToken).ConfigureAwait(false);
        if (assignments.TotalCount > 0)
            return true;

        var readings = await _readings.SearchAsync(
            q => q.Where(r => r.DeviceId == deviceId),
            1,
            1,
            true,
            cancellationToken).ConfigureAwait(false);
        if (readings.TotalCount > 0)
            return true;

        var emergencies = await _emergencies.SearchAsync(
            q => q.Where(e => e.DeviceId == deviceId),
            1,
            1,
            true,
            cancellationToken).ConfigureAwait(false);
        return emergencies.TotalCount > 0;
    }
}
