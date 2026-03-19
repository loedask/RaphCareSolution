namespace RaphCare.Application.Common.Interfaces;

/// <summary>
/// Audit logger abstraction for FHIR (Fast Healthcare Interoperability Resources) export requests.
/// Phase 1 does not persist to a database; it uses structured logging.
/// </summary>
public interface IFhirExportAuditLogger
{
    void LogExportAttempt(
        string resourceType,
        Guid resourceId,
        Guid? requestedByUserId,
        DateTime requestedAtUtc,
        Guid? clinicId,
        bool success,
        string? errorMessage);
}

