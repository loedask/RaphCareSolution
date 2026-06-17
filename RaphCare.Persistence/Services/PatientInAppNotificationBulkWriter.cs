using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.Patients;

namespace RaphCare.Persistence.Services;

public sealed class PatientInAppNotificationBulkWriter(ClinicalDbContext db) : IPatientInAppNotificationBulkWriter
{
    private readonly ClinicalDbContext _db = db;

    public Task<int> MarkAllReadForPatientAsync(Guid patientId, CancellationToken cancellationToken = default) =>
        _db.Set<PatientInAppNotification>()
            .Where(n => n.PatientId == patientId && !n.IsRead)
            .ExecuteUpdateAsync(
                s => s.SetProperty(n => n.IsRead, true).SetProperty(n => n.UpdatedAt, DateTime.UtcNow),
                cancellationToken);
}
