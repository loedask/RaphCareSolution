using FluentValidation.Results;
using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Devices;
using ValidationException = RaphCare.Application.Common.Exceptions.ValidationException;

namespace RaphCare.Application.Features.Devices.Commands.UnassignDeviceFromPatient;

public sealed class UnassignDeviceFromPatientHandler : IRequestHandler<UnassignDeviceFromPatientCommand, Guid>
{
    private readonly IRepository<Device> _devices;
    private readonly IRepository<DeviceAssignment> _assignments;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _clock;

    public UnassignDeviceFromPatientHandler(
        IRepository<Device> devices,
        IRepository<DeviceAssignment> assignments,
        IUnitOfWork unitOfWork,
        IDateTimeProvider clock)
    {
        _devices = devices;
        _assignments = assignments;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<Guid> Handle(UnassignDeviceFromPatientCommand request, CancellationToken cancellationToken)
    {
        var device = await _devices.GetByIdAsync(request.DeviceId, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException(nameof(Device), request.DeviceId);

        var activePage = await _assignments.SearchAsync(
            q => q.Where(a => a.DeviceId == device.Id && a.IsActive),
            1,
            20,
            true,
            cancellationToken).ConfigureAwait(false);

        if (activePage.Items.Count == 0 && !device.IsAssigned)
        {
            throw new ValidationException(
            [
                new ValidationFailure(nameof(UnassignDeviceFromPatientCommand.DeviceId), "Device is not assigned to a patient.")
            ]);
        }

        var now = _clock.UtcNow;
        foreach (var assignment in activePage.Items)
        {
            assignment.IsActive = false;
            assignment.ReturnedAt = now;
            await _assignments.UpdateAsync(assignment, cancellationToken).ConfigureAwait(false);
        }

        device.IsAssigned = false;
        device.Status = "InStock";
        await _devices.UpdateAsync(device, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return device.Id;
    }
}
