using System.Text.Json;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Patients;
using RaphCare.Domain.Patients.Enums;

namespace RaphCare.Persistence;

/// <summary>
/// Persistence implementation of IPatientIdentityTimelineService.
/// Adds identity timeline rows to the clinical DbContext; callers are responsible for SaveChanges.
/// </summary>
public class PatientIdentityTimelineService(
    ClinicalDbContext clinicalDbContext,
    IDateTimeProvider clock) : IPatientIdentityTimelineService
{
    private readonly ClinicalDbContext _clinicalDbContext = clinicalDbContext ?? throw new ArgumentNullException(nameof(clinicalDbContext));
    private readonly IDateTimeProvider _clock = clock ?? throw new ArgumentNullException(nameof(clock));

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public Task RecordEventAsync(
        Guid patientId,
        PatientIdentityEventType eventType,
        object? eventData,
        Guid? performedByUserId,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var json = JsonSerializer.Serialize(eventData ?? new { }, JsonOptions);

        _clinicalDbContext.PatientIdentityEvents.Add(new PatientIdentityEvent
        {
            PatientId = patientId,
            EventType = eventType,
            EventDataJson = json,
            OccurredAt = _clock.UtcNow,
            PerformedByUserId = performedByUserId
        });

        return Task.CompletedTask;
    }
}

