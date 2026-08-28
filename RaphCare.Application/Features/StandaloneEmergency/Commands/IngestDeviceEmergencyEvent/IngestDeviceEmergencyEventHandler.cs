using System.Globalization;
using System.Text;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.StandaloneEmergency.DTOs;
using RaphCare.Domain.Devices;
using RaphCare.Domain.Patients;

namespace RaphCare.Application.Features.StandaloneEmergency.Commands.IngestDeviceEmergencyEvent;

public sealed class IngestDeviceEmergencyEventHandler(
    IRepository<Device> devices,
    IRepository<DeviceAssignment> assignments,
    IRepository<DeviceEmergencyEvent> events,
    IRepository<Patient> patients,
    IUnitOfWork unitOfWork,
    ISmsService smsService,
    IDateTimeProvider clock) : IRequestHandler<IngestDeviceEmergencyEventCommand, IngestDeviceEmergencyEventResult>
{
    private readonly IRepository<Device> _devices = devices;
    private readonly IRepository<DeviceAssignment> _assignments = assignments;
    private readonly IRepository<DeviceEmergencyEvent> _events = events;
    private readonly IRepository<Patient> _patients = patients;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ISmsService _smsService = smsService;
    private readonly IDateTimeProvider _clock = clock;

    public async Task<IngestDeviceEmergencyEventResult> Handle(
        IngestDeviceEmergencyEventCommand request,
        CancellationToken cancellationToken)
    {
        var serial = request.SerialNumber.Trim();
        var devicePage = await _devices.SearchAsync(
            q => q.Where(d => d.SerialNumber == serial),
            1,
            1,
            true,
            cancellationToken).ConfigureAwait(false);

        var device = (devicePage.Items.Count > 0 ? devicePage.Items[0] : null)
            ?? throw new NotFoundException(nameof(Device), serial);

        if (!string.IsNullOrWhiteSpace(request.ExternalEventId))
        {
            var ext = request.ExternalEventId.Trim();
            var dup = await _events.SearchAsync(
                q => q.Where(e => e.DeviceId == device.Id && e.ExternalCorrelationId == ext),
                1,
                1,
                true,
                cancellationToken).ConfigureAwait(false);
            if (dup.TotalCount > 0)
            {
                var existing = dup.Items[0];
                return new IngestDeviceEmergencyEventResult { Id = existing.Id, WasDuplicate = true };
            }
        }

        var assignmentPage = await _assignments.SearchAsync(
            q => q
                .Where(a => a.DeviceId == device.Id && a.IsActive && a.ReturnedAt == null)
                .OrderByDescending(a => a.AssignedAt),
            1,
            1,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        var assignment = assignmentPage.Items.Count > 0 ? assignmentPage.Items[0] : null;
        if (assignment is null)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure(nameof(Device), "No active patient assignment for this device serial number.")
            });
        }

        var occurredUtc = NormalizeUtc(request.OccurredAtUtc);
        var receivedUtc = _clock.UtcNow;

        var entity = new DeviceEmergencyEvent
        {
            DeviceId = device.Id,
            PatientId = assignment.PatientId,
            ClinicId = device.ClinicId,
            EventType = request.EventType.Trim(),
            OccurredAtUtc = occurredUtc,
            ReceivedAtUtc = receivedUtc,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            HorizontalAccuracyMeters = request.HorizontalAccuracyMeters,
            ExternalCorrelationId = string.IsNullOrWhiteSpace(request.ExternalEventId)
                ? null
                : request.ExternalEventId.Trim()
        };

        await _events.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await NotifyCaregiversAsync(entity, device, cancellationToken).ConfigureAwait(false);

        return new IngestDeviceEmergencyEventResult { Id = entity.Id, WasDuplicate = false };
    }

    private static DateTime NormalizeUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Utc),
            _ => value.ToUniversalTime()
        };
    }

    private async Task NotifyCaregiversAsync(
        DeviceEmergencyEvent entity,
        Device device,
        CancellationToken cancellationToken)
    {
        var patientPage = await _patients.SearchAsync(
            q => q
                .Where(p => p.Id == entity.PatientId)
                .Include(p => p.EmergencyContacts)
                .OrderBy(p => p.Id),
            1,
            1,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        var patient = patientPage.Items.Count > 0 ? patientPage.Items[0] : null;
        if (patient is null)
        {
            entity.CaregiversNotified = false;
            entity.CaregiverNotificationSummary = "Patient not found";
            await _events.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        var name = string.Join(' ', new[] { patient.FirstName, patient.LastName }.Where(s => !string.IsNullOrWhiteSpace(s)));
        if (string.IsNullOrWhiteSpace(name))
            name = "Patient";

        var loc = FormatLocation(entity);
        var message = new StringBuilder(320)
            .Append(CultureInfo.InvariantCulture, $"RaphCare SafeCare: {entity.EventType} alert for {name} at {entity.OccurredAtUtc:u}.")
            .Append(CultureInfo.InvariantCulture, $" Device {device.Model} (S/N {device.SerialNumber}).")
            .Append(loc)
            .ToString();

        var phones = patient.EmergencyContacts
            .Select(c => c.PhoneNumber?.Trim())
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (phones.Count == 0)
        {
            entity.CaregiversNotified = false;
            entity.CaregiverNotificationSummary = "No emergency contact phone numbers";
            await _events.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        var sent = 0;
        foreach (var phone in phones)
        {
            await _smsService.SendSmsAsync(phone!, message, cancellationToken).ConfigureAwait(false);
            sent++;
        }

        entity.CaregiversNotified = true;
        entity.CaregiversNotifiedAtUtc = _clock.UtcNow;
        entity.CaregiverNotificationSummary = $"SMS:{sent}";
        await _events.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static string FormatLocation(DeviceEmergencyEvent entity)
    {
        if (entity.Latitude is null || entity.Longitude is null)
            return " Location: unknown.";

        return string.Create(CultureInfo.InvariantCulture, $" Location: {entity.Latitude:F5},{entity.Longitude:F5}.");
    }
}
