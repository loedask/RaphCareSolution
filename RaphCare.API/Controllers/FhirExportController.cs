using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Interoperability.Queries.GetFhirAppointments;
using RaphCare.Application.Features.Interoperability.Queries.GetFhirEncounters;
using RaphCare.Application.Features.Interoperability.Queries.GetFhirPatientById;
using RaphCare.Application.Features.Interoperability.Queries.GetFhirEncounterById;
using RaphCare.Application.Features.Interoperability.Queries.GetFhirAppointmentById;
using RaphCare.Application.Features.Interoperability.Queries.GetFhirPatients;
using RaphCare.Application.Features.Interoperability.Queries.GetFhirOrganizations;
using RaphCare.Application.Features.Interoperability.Queries.GetFhirOrganizationById;
using RaphCare.Application.Features.Interoperability.Queries.GetFhirObservationById;
using RaphCare.Application.Features.Interoperability.Queries.GetFhirObservations;

namespace RaphCare.API.Controllers;

/// <summary>
/// Phase 1 FHIR (Fast Healthcare Interoperability Resources)-shaped exports.
/// Security: Provider/Admin only. Patient-facing access is intentionally not exposed yet.
/// </summary>
[Authorize(Policy = "RequireProvider")]
[ApiController]
[Route("api/fhir")]
public class FhirExportController(
    IMediator mediator,
    ICurrentUserService currentUserService,
    IClinicContext clinicContext,
    IFhirExportAuditLogger auditLogger) : ControllerBase
{
    private static bool IsFhirJsonRequest(HttpRequest request)
        => request.Headers.Accept.ToString().Contains("application/fhir+json", StringComparison.OrdinalIgnoreCase);

    private void ApplyFhirJsonContentTypeIfRequested()
    {
        if (IsFhirJsonRequest(Request))
            Response.ContentType = "application/fhir+json";
    }

    [HttpGet("patients/{id:guid}", Name = "GetFhirPatientById")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPatient([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        ApplyFhirJsonContentTypeIfRequested();
        var requestedAtUtc = DateTime.UtcNow;
        var clinicId = clinicContext.ClinicId;
        var requestedByUserId = currentUserService.CurrentUserId;
        try
        {
            var result = await mediator.Send(new GetFhirPatientByIdQuery { Id = id }, cancellationToken).ConfigureAwait(false);
            auditLogger.LogExportAttempt(
                resourceType: "Patient",
                resourceId: id,
                requestedByUserId: requestedByUserId,
                requestedAtUtc: requestedAtUtc,
                clinicId: clinicId,
                success: true,
                errorMessage: null);

            return Ok(result);
        }
        catch (Exception ex)
        {
            auditLogger.LogExportAttempt(
                resourceType: "Patient",
                resourceId: id,
                requestedByUserId: requestedByUserId,
                requestedAtUtc: requestedAtUtc,
                clinicId: clinicId,
                success: false,
                errorMessage: ex.Message);
            throw;
        }
    }

    [HttpGet("encounters/{id:guid}", Name = "GetFhirEncounterById")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEncounter([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        ApplyFhirJsonContentTypeIfRequested();
        var requestedAtUtc = DateTime.UtcNow;
        var clinicId = clinicContext.ClinicId;
        var requestedByUserId = currentUserService.CurrentUserId;
        try
        {
            var result = await mediator.Send(new GetFhirEncounterByIdQuery { Id = id }, cancellationToken).ConfigureAwait(false);
            auditLogger.LogExportAttempt(
                resourceType: "Encounter",
                resourceId: id,
                requestedByUserId: requestedByUserId,
                requestedAtUtc: requestedAtUtc,
                clinicId: clinicId,
                success: true,
                errorMessage: null);

            return Ok(result);
        }
        catch (Exception ex)
        {
            auditLogger.LogExportAttempt(
                resourceType: "Encounter",
                resourceId: id,
                requestedByUserId: requestedByUserId,
                requestedAtUtc: requestedAtUtc,
                clinicId: clinicId,
                success: false,
                errorMessage: ex.Message);
            throw;
        }
    }

    [HttpGet("organizations/{id:guid}", Name = "GetFhirOrganizationById")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrganization([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        ApplyFhirJsonContentTypeIfRequested();
        var requestedAtUtc = DateTime.UtcNow;
        var clinicId = clinicContext.ClinicId;
        var requestedByUserId = currentUserService.CurrentUserId;
        try
        {
            var result = await mediator.Send(new GetFhirOrganizationByIdQuery { Id = id }, cancellationToken).ConfigureAwait(false);
            auditLogger.LogExportAttempt(
                resourceType: "Organization",
                resourceId: id,
                requestedByUserId: requestedByUserId,
                requestedAtUtc: requestedAtUtc,
                clinicId: clinicId,
                success: true,
                errorMessage: null);

            return Ok(result);
        }
        catch (Exception ex)
        {
            auditLogger.LogExportAttempt(
                resourceType: "Organization",
                resourceId: id,
                requestedByUserId: requestedByUserId,
                requestedAtUtc: requestedAtUtc,
                clinicId: clinicId,
                success: false,
                errorMessage: ex.Message);
            throw;
        }
    }

    [HttpGet("appointments/{id:guid}", Name = "GetFhirAppointmentById")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAppointment([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        ApplyFhirJsonContentTypeIfRequested();
        var requestedAtUtc = DateTime.UtcNow;
        var clinicId = clinicContext.ClinicId;
        var requestedByUserId = currentUserService.CurrentUserId;
        try
        {
            var result = await mediator.Send(new GetFhirAppointmentByIdQuery { Id = id }, cancellationToken).ConfigureAwait(false);
            auditLogger.LogExportAttempt(
                resourceType: "Appointment",
                resourceId: id,
                requestedByUserId: requestedByUserId,
                requestedAtUtc: requestedAtUtc,
                clinicId: clinicId,
                success: true,
                errorMessage: null);

            return Ok(result);
        }
        catch (Exception ex)
        {
            auditLogger.LogExportAttempt(
                resourceType: "Appointment",
                resourceId: id,
                requestedByUserId: requestedByUserId,
                requestedAtUtc: requestedAtUtc,
                clinicId: clinicId,
                success: false,
                errorMessage: ex.Message);
            throw;
        }
    }

    [HttpGet("patients", Name = "SearchFhirPatients")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchPatients(
        [FromQuery] Guid? id,
        [FromQuery] string? nationalHealthId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        ApplyFhirJsonContentTypeIfRequested();
        var requestedAtUtc = DateTime.UtcNow;
        var clinicId = clinicContext.ClinicId;
        var requestedByUserId = currentUserService.CurrentUserId;
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? 20 : Math.Min(pageSize, 100);

        try
        {
            var result = await mediator.Send(
                new GetFhirPatientsQuery { Id = id, NationalHealthId = nationalHealthId, PageNumber = pageNumber, PageSize = pageSize },
                cancellationToken).ConfigureAwait(false);

            auditLogger.LogExportAttempt(
                resourceType: "Patient",
                resourceId: Guid.Empty,
                requestedByUserId: requestedByUserId,
                requestedAtUtc: requestedAtUtc,
                clinicId: clinicId,
                success: true,
                errorMessage: null);

            return Ok(result);
        }
        catch (Exception ex)
        {
            auditLogger.LogExportAttempt(
                resourceType: "Patient",
                resourceId: Guid.Empty,
                requestedByUserId: requestedByUserId,
                requestedAtUtc: requestedAtUtc,
                clinicId: clinicId,
                success: false,
                errorMessage: ex.Message);
            throw;
        }
    }

    [HttpGet("encounters", Name = "SearchFhirEncounters")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchEncounters(
        [FromQuery] Guid? patientId,
        [FromQuery] Guid? clinicId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        ApplyFhirJsonContentTypeIfRequested();
        var requestedAtUtc = DateTime.UtcNow;
        var requestedByUserId = currentUserService.CurrentUserId;
        var auditClinicId = clinicContext.ClinicId;
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? 20 : Math.Min(pageSize, 100);

        try
        {
            var result = await mediator.Send(
                new GetFhirEncountersQuery { PatientId = patientId, ClinicId = clinicId, PageNumber = pageNumber, PageSize = pageSize },
                cancellationToken).ConfigureAwait(false);

            auditLogger.LogExportAttempt(
                resourceType: "Encounter",
                resourceId: Guid.Empty,
                requestedByUserId: requestedByUserId,
                requestedAtUtc: requestedAtUtc,
                clinicId: auditClinicId,
                success: true,
                errorMessage: null);

            return Ok(result);
        }
        catch (Exception ex)
        {
            auditLogger.LogExportAttempt(
                resourceType: "Encounter",
                resourceId: Guid.Empty,
                requestedByUserId: requestedByUserId,
                requestedAtUtc: requestedAtUtc,
                clinicId: auditClinicId,
                success: false,
                errorMessage: ex.Message);
            throw;
        }
    }

    [HttpGet("appointments", Name = "SearchFhirAppointments")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchAppointments(
        [FromQuery] Guid? patientId,
        [FromQuery] string? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        ApplyFhirJsonContentTypeIfRequested();
        var requestedAtUtc = DateTime.UtcNow;
        var clinicId = clinicContext.ClinicId;
        var requestedByUserId = currentUserService.CurrentUserId;
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? 20 : Math.Min(pageSize, 100);

        try
        {
            var result = await mediator.Send(
                new GetFhirAppointmentsQuery { PatientId = patientId, Status = status, PageNumber = pageNumber, PageSize = pageSize },
                cancellationToken).ConfigureAwait(false);

            auditLogger.LogExportAttempt(
                resourceType: "Appointment",
                resourceId: Guid.Empty,
                requestedByUserId: requestedByUserId,
                requestedAtUtc: requestedAtUtc,
                clinicId: clinicId,
                success: true,
                errorMessage: null);

            return Ok(result);
        }
        catch (Exception ex)
        {
            auditLogger.LogExportAttempt(
                resourceType: "Appointment",
                resourceId: Guid.Empty,
                requestedByUserId: requestedByUserId,
                requestedAtUtc: requestedAtUtc,
                clinicId: clinicId,
                success: false,
                errorMessage: ex.Message);
            throw;
        }
    }

    [HttpGet("organizations", Name = "SearchFhirOrganizations")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchOrganizations(
        [FromQuery] bool? active,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        ApplyFhirJsonContentTypeIfRequested();
        var requestedAtUtc = DateTime.UtcNow;
        var clinicId = clinicContext.ClinicId;
        var requestedByUserId = currentUserService.CurrentUserId;
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? 20 : Math.Min(pageSize, 100);

        try
        {
            var result = await mediator.Send(
                new GetFhirOrganizationsQuery { Active = active, PageNumber = pageNumber, PageSize = pageSize },
                cancellationToken).ConfigureAwait(false);

            auditLogger.LogExportAttempt(
                resourceType: "Organization",
                resourceId: Guid.Empty,
                requestedByUserId: requestedByUserId,
                requestedAtUtc: requestedAtUtc,
                clinicId: clinicId,
                success: true,
                errorMessage: null);

            return Ok(result);
        }
        catch (Exception ex)
        {
            auditLogger.LogExportAttempt(
                resourceType: "Organization",
                resourceId: Guid.Empty,
                requestedByUserId: requestedByUserId,
                requestedAtUtc: requestedAtUtc,
                clinicId: clinicId,
                success: false,
                errorMessage: ex.Message);
            throw;
        }
    }

    [HttpGet("observations/{id:guid}", Name = "GetFhirObservationById")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetObservation([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        ApplyFhirJsonContentTypeIfRequested();
        var requestedAtUtc = DateTime.UtcNow;
        var clinicId = clinicContext.ClinicId;
        var requestedByUserId = currentUserService.CurrentUserId;
        try
        {
            var result = await mediator.Send(new GetFhirObservationByIdQuery { Id = id }, cancellationToken).ConfigureAwait(false);
            auditLogger.LogExportAttempt(
                resourceType: "Observation",
                resourceId: id,
                requestedByUserId: requestedByUserId,
                requestedAtUtc: requestedAtUtc,
                clinicId: clinicId,
                success: true,
                errorMessage: null);

            return Ok(result);
        }
        catch (Exception ex)
        {
            auditLogger.LogExportAttempt(
                resourceType: "Observation",
                resourceId: id,
                requestedByUserId: requestedByUserId,
                requestedAtUtc: requestedAtUtc,
                clinicId: clinicId,
                success: false,
                errorMessage: ex.Message);
            throw;
        }
    }

    [HttpGet("observations", Name = "SearchFhirObservations")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchObservations(
        [FromQuery] Guid patientId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? readingType = null,
        [FromQuery] DateTime? recordedFromUtc = null,
        [FromQuery] DateTime? recordedToUtc = null,
        CancellationToken cancellationToken = default)
    {
        ApplyFhirJsonContentTypeIfRequested();
        var requestedAtUtc = DateTime.UtcNow;
        var clinicId = clinicContext.ClinicId;
        var requestedByUserId = currentUserService.CurrentUserId;
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? 20 : Math.Min(pageSize, 100);

        try
        {
            var result = await mediator.Send(
                new GetFhirObservationsQuery
                {
                    PatientId = patientId,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    ReadingType = readingType,
                    RecordedFromUtc = recordedFromUtc,
                    RecordedToUtc = recordedToUtc
                },
                cancellationToken).ConfigureAwait(false);

            auditLogger.LogExportAttempt(
                resourceType: "Observation",
                resourceId: Guid.Empty,
                requestedByUserId: requestedByUserId,
                requestedAtUtc: requestedAtUtc,
                clinicId: clinicId,
                success: true,
                errorMessage: null);

            return Ok(result);
        }
        catch (Exception ex)
        {
            auditLogger.LogExportAttempt(
                resourceType: "Observation",
                resourceId: Guid.Empty,
                requestedByUserId: requestedByUserId,
                requestedAtUtc: requestedAtUtc,
                clinicId: clinicId,
                success: false,
                errorMessage: ex.Message);
            throw;
        }
    }
}

