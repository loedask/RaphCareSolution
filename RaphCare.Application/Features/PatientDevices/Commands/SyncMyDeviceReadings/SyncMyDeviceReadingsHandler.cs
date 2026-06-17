using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.PatientDevices.DTOs;
using RaphCare.Domain.Devices;

namespace RaphCare.Application.Features.PatientDevices.Commands.SyncMyDeviceReadings;

public sealed class SyncMyDeviceReadingsHandler : IRequestHandler<SyncMyDeviceReadingsCommand, SyncMyDeviceReadingsResponseDto>
{
    private readonly IRepository<Device> _devices;
    private readonly IRepository<DeviceAssignment> _assignments;
    private readonly IRepository<DeviceReading> _readings;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public SyncMyDeviceReadingsHandler(
        IRepository<Device> devices,
        IRepository<DeviceAssignment> assignments,
        IRepository<DeviceReading> readings,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _devices = devices;
        _assignments = assignments;
        _readings = readings;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<SyncMyDeviceReadingsResponseDto> Handle(SyncMyDeviceReadingsCommand request, CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required.");

        var device = await _devices.GetByIdAsync(request.DeviceId, cancellationToken).ConfigureAwait(false);
        if (device is null)
            throw new NotFoundException(nameof(Device), request.DeviceId);

        var assignmentOk = await _assignments.SearchAsync(
            q => q.Where(a => a.DeviceId == request.DeviceId && a.PatientId == patientId && a.IsActive),
            1,
            1,
            true,
            cancellationToken).ConfigureAwait(false);

        if (assignmentOk.Items.Count == 0)
            throw new NotFoundException(nameof(Device), request.DeviceId);

        var receivedAt = _clock.UtcNow;
        var hrCount = 0;
        var spo2Count = 0;

        foreach (var pt in request.HeartRates ?? Enumerable.Empty<HeartRatePointDto>())
        {
            var bpm = pt.BeatsPerMinute;
            var row = new HeartRateReading
            {
                DeviceId = device.Id,
                PatientId = patientId,
                RecordedAt = pt.RecordedAt,
                ReceivedAt = receivedAt,
                ReadingType = "HeartRate",
                Unit = "bpm",
                PrimaryValue = bpm,
                HeartRate = bpm
            };
            await _readings.AddAsync(row, cancellationToken).ConfigureAwait(false);
            hrCount++;
        }

        foreach (var pt in request.Spo2 ?? Enumerable.Empty<Spo2PointDto>())
        {
            var pulse = pt.PulseRate ?? 0;
            var row = new PulseOximeterReading
            {
                DeviceId = device.Id,
                PatientId = patientId,
                RecordedAt = pt.RecordedAt,
                ReceivedAt = receivedAt,
                ReadingType = "SpO2",
                Unit = "%",
                PrimaryValue = pt.SpO2,
                SpO2 = pt.SpO2,
                PulseRate = pulse
            };
            await _readings.AddAsync(row, cancellationToken).ConfigureAwait(false);
            spo2Count++;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new SyncMyDeviceReadingsResponseDto { HeartRateCount = hrCount, SpO2Count = spo2Count };
    }
}
