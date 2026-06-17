using MediatR;
using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.PatientDevices.DTOs;
using RaphCare.Domain.Devices;

namespace RaphCare.Application.Features.PatientDevices.Queries.GetMyDevices;

public sealed class GetMyDevicesHandler : IRequestHandler<GetMyDevicesQuery, IReadOnlyList<PatientDeviceListItemDto>>
{
    private readonly IRepository<DeviceAssignment> _assignments;
    private readonly ICurrentUserService _currentUser;

    public GetMyDevicesHandler(IRepository<DeviceAssignment> assignments, ICurrentUserService currentUser)
    {
        _assignments = assignments;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<PatientDeviceListItemDto>> Handle(GetMyDevicesQuery request, CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required.");

        var paged = await _assignments.SearchAsync(
            q => q
                .Where(a => a.PatientId == patientId && a.IsActive)
                .Include(a => a.Device)
                .OrderByDescending(a => a.AssignedAt),
            1,
            100,
            false,
            cancellationToken).ConfigureAwait(false);

        return paged.Items
            .Where(a => a.Device != null)
            .Select(a => new PatientDeviceListItemDto
            {
                DeviceId = a.DeviceId,
                SerialNumber = a.Device!.SerialNumber,
                Model = a.Device.Model,
                AssignedAt = a.AssignedAt
            })
            .ToList();
    }
}
