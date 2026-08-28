using Microsoft.Extensions.Logging;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.API.App.Services;

/// <summary>
/// Structured log-based audit for FHIR (Fast Healthcare Interoperability Resources) export requests.
/// </summary>
public sealed partial class FhirExportAuditLogger(ILogger<FhirExportAuditLogger> logger) : IFhirExportAuditLogger
{
    public void LogExportAttempt(
        string resourceType,
        Guid resourceId,
        Guid? requestedByUserId,
        DateTime requestedAtUtc,
        Guid? clinicId,
        bool success,
        string? errorMessage)
    {
        LogFhirExport(
            resourceType,
            resourceId,
            requestedByUserId,
            clinicId,
            requestedAtUtc,
            success,
            errorMessage);
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "FhirExport: ResourceType={ResourceType}, ResourceId={ResourceId}, RequestedByUserId={RequestedByUserId}, ClinicId={ClinicId}, RequestedAt={RequestedAt:O}, Success={Success}, Error={ErrorMessage}")]
    private partial void LogFhirExport(
        string resourceType,
        Guid resourceId,
        Guid? requestedByUserId,
        Guid? clinicId,
        DateTime requestedAt,
        bool success,
        string? errorMessage);
}
