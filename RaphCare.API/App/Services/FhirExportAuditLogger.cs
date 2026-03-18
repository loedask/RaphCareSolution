using Microsoft.Extensions.Logging;
using RaphCare.Application.Common.Interfaces;

namespace RaphCare.API.App.Services;

/// <summary>
/// Structured log-based audit for FHIR export requests.
/// </summary>
public class FhirExportAuditLogger(ILogger<FhirExportAuditLogger> logger) : IFhirExportAuditLogger
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
        logger.LogInformation(
            "FhirExport: ResourceType={ResourceType}, ResourceId={ResourceId}, RequestedByUserId={RequestedByUserId}, ClinicId={ClinicId}, RequestedAt={RequestedAt:O}, Success={Success}, Error={ErrorMessage}",
            resourceType,
            resourceId,
            requestedByUserId,
            clinicId,
            requestedAtUtc,
            success,
            errorMessage);
    }
}

