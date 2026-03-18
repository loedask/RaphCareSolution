using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Interoperability.Queries.GetFhirPatientById;
using RaphCare.Application.Features.Interoperability.Queries.GetFhirEncounterById;
using RaphCare.Application.Features.Interoperability.Queries.GetFhirOrganizationById;

namespace RaphCare.API.Controllers;

/// <summary>
/// Phase 1 FHIR-shaped exports.
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
    // TODO: future support for content negotiation (application/fhir+json).

    [HttpGet("patients/{id:guid}", Name = "GetFhirPatientById")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPatient([FromRoute] Guid id, CancellationToken cancellationToken)
    {
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
}

