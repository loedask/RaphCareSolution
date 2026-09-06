using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Features.Organization.Commands.AdmitAdminClinicPatient;
using RaphCare.Application.Features.Organization.Commands.AssignAdminClinicDeviceToPatient;
using RaphCare.Application.Features.Organization.Commands.CreateAdminClinicBed;
using RaphCare.Application.Features.Organization.Commands.CreateAdminClinicRoom;
using RaphCare.Application.Features.Organization.Commands.CreateAdminClinicWard;
using RaphCare.Application.Features.Organization.Commands.DeleteAdminClinicBed;
using RaphCare.Application.Features.Organization.Commands.DeleteAdminClinicRoom;
using RaphCare.Application.Features.Organization.Commands.DeleteAdminClinicWard;
using RaphCare.Application.Features.Organization.Commands.DischargeAdminClinicAdmission;
using RaphCare.Application.Features.Organization.Commands.DraftAdminClinicDischargeSummary;
using RaphCare.Application.Features.Organization.Commands.CreateAdminClinicAdmissionObservation;
using RaphCare.Application.Features.Organization.Queries.GetAdminClinicAdmissionObservations;
using RaphCare.Application.Features.Organization.Commands.SetAdminClinicBedStatus;
using RaphCare.Application.Features.Organization.Commands.TransferAdminClinicAdmission;
using RaphCare.Application.Features.Organization.Commands.UpdateAdminClinicBed;
using RaphCare.Application.Features.Organization.Commands.UpdateAdminClinicRoom;
using RaphCare.Application.Features.Organization.Commands.UpdateAdminClinicWard;
using RaphCare.Application.Features.Organization.Queries.GetAdminClinicAdmissionById;
using RaphCare.Application.Features.Organization.Queries.GetAdminClinicAdmissions;
using RaphCare.Application.Features.Organization.Queries.GetAdminClinicInpatientBoard;
using RaphCare.Application.Features.Organization.Commands.CancelAdminClinicAppointment;
using RaphCare.Application.Features.Organization.Commands.CallAdminClinicLabOrder;
using RaphCare.Application.Features.Organization.Commands.CallAdminClinicPrescription;
using RaphCare.Application.Features.Organization.Commands.CallAdminClinicCasualtyTicket;
using RaphCare.Application.Features.Organization.Commands.CompleteAdminClinicCasualtyTicket;
using RaphCare.Application.Features.Organization.Commands.CreateAdminClinicCasualtyTicket;
using RaphCare.Application.Features.Organization.Commands.CreateAdminClinicTheatreCase;
using RaphCare.Application.Features.Organization.Commands.CreateAdminClinicReferral;
using RaphCare.Application.Features.Organization.Commands.CreateAdminClinicRosterEntry;
using RaphCare.Application.Features.Organization.Commands.DeleteAdminClinicRosterEntry;
using RaphCare.Application.Features.Organization.Commands.EnsureCasualtyDisplayToken;
using RaphCare.Application.Features.Organization.Commands.EnsureCollectionDisplayToken;
using RaphCare.Application.Features.Organization.Commands.UpdateAdminClinicTheatreCaseStatus;
using RaphCare.Application.Features.Organization.Commands.UpdateAdminClinicReferralStatus;
using RaphCare.Application.Features.Organization.Queries.GetAdminClinicCasualtyBoard;
using RaphCare.Application.Features.Organization.Queries.GetAdminClinicTheatreBoard;
using RaphCare.Application.Features.Organization.Queries.GetAdminClinicReferralBoard;
using RaphCare.Application.Features.Organization.Queries.GetAdminClinicRosterBoard;
using RaphCare.Application.Features.Organization.Commands.SetAdminClinicProviderActive;
using RaphCare.Application.Features.Organization.Commands.StartAdminClinicTeleSession;
using RaphCare.Application.Features.Organization.Commands.CreateAdminClinicAppointment;
using RaphCare.Application.Features.Organization.Commands.CreateAdminClinicProvider;
using RaphCare.Application.Features.Organization.Commands.CreateAdminClinicProviderSchedule;
using RaphCare.Application.Features.Organization.Commands.DeleteAdminClinicProviderSchedule;
using RaphCare.Application.Features.Organization.Commands.CreateAdminFacility;
using RaphCare.Application.Features.Organization.Commands.DeleteAdminFacility;
using RaphCare.Application.Features.Organization.Commands.EnsureClinicMembership;
using RaphCare.Application.Features.Organization.Commands.GrantAdminClinicPatientAccess;
using RaphCare.Application.Features.Organization.Commands.InviteClinicStaff;
using RaphCare.Application.Features.Organization.Commands.RevokeAdminClinicPatientAccess;
using RaphCare.Application.Features.Organization.Commands.RescheduleAdminClinicAppointment;
using RaphCare.Application.Features.Organization.Commands.RegisterClinic;
using RaphCare.Application.Features.Organization.Commands.RemoveClinicStaff;
using RaphCare.Application.Features.Organization.Commands.ResendClinicStaffInvitation;
using RaphCare.Application.Features.Organization.Commands.UpdateAdminFacility;
using RaphCare.Application.Features.Organization.Commands.UpdateAdminClinic;
using RaphCare.Application.Features.Organization.Commands.UpdateClinicStaffRole;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Application.Features.Organization.Commands.CancelAdminClinicLabOrder;
using RaphCare.Application.Features.Organization.Commands.CancelAdminClinicPrescription;
using RaphCare.Application.Features.Organization.Commands.CompleteAdminClinicLabOrder;
using RaphCare.Application.Features.Organization.Commands.CompleteAdminClinicVisit;
using RaphCare.Application.Features.Organization.Commands.CreateAdminClinicVisitLabResult;
using RaphCare.Application.Features.Organization.Commands.CreateAdminClinicVisitNote;
using RaphCare.Application.Features.Organization.Commands.CreateAdminClinicVisitPrescription;
using RaphCare.Application.Features.Organization.Commands.CreateAdminClinicVisitVital;
using RaphCare.Application.Features.Organization.Commands.DispenseAdminClinicPrescription;
using RaphCare.Application.Features.Organization.Commands.StartAdminClinicVisit;
using RaphCare.Application.Features.Organization.Commands.UndoAdminClinicLabOrder;
using RaphCare.Application.Features.Organization.Commands.UndoAdminClinicPrescription;
using RaphCare.Application.Features.Organization.Commands.UpsertAdminClinicVisitSoapNote;
using RaphCare.Application.Features.Organization.Queries.GetAdminClinicCollectionOrders;
using RaphCare.Application.Features.Organization.Queries.GetAdminClinicDevices;
using RaphCare.Application.Features.Organization.Queries.GetAdminClinicVisitById;
using RaphCare.Application.Features.Organization.Queries.GetAdminClinicById;
using RaphCare.Application.Features.Organization.Queries.GetAdminClinics;
using RaphCare.Application.Features.Organization.Queries.GetAdminClinicStaff;
using RaphCare.Application.Features.Organization.Queries.GetAdminClinicAppointments;
using RaphCare.Application.Features.Organization.Queries.GetAdminClinicDashboard;
using RaphCare.Application.Features.Organization.Queries.GetAdminClinicProviderById;
using RaphCare.Application.Features.Organization.Queries.GetAdminClinicProviders;
using RaphCare.Application.Features.Organization.Queries.GetAdminClinicPatientById;
using RaphCare.Application.Features.Organization.Queries.GetAdminClinicPatientPhoto;
using RaphCare.Application.Features.Organization.Queries.GetAdminClinicPatients;

namespace RaphCare.API.Controllers;

/// <summary>Practice portal: hospital onboarding and directory. Does not require X-Clinic-Id.</summary>
[Authorize(Policy = "RequireProvider")]
[ApiController]
[Route("api/admin/clinics")]
public class AdminClinicsController(IMediator mediator) : ControllerBase
{
    /// <summary>List hospitals the signed-in professional is linked to.</summary>
    [HttpGet(Name = "GetAdminClinics")]
    [ProducesResponseType(typeof(PagedResult<ClinicListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new GetAdminClinicsQuery { PageNumber = pageNumber, PageSize = pageSize },
            cancellationToken);
        return Ok(result);
    }

    /// <summary>Get one hospital (must be linked to the current user).</summary>
    [HttpGet("{id:guid}", Name = "GetAdminClinicById")]
    [ProducesResponseType(typeof(ClinicDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAdminClinicByIdQuery { ClinicId = id }, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Update hospital profile fields (name, country, time zone).</summary>
    [HttpPut("{id:guid}", Name = "UpdateAdminClinic")]
    [ProducesResponseType(typeof(ClinicDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateAdminClinicRequest body,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateAdminClinicCommand
            {
                ClinicId = id,
                Name = body.Name,
                Country = body.Country,
                TimeZone = body.TimeZone,
                IsActive = body.IsActive,
                AllowAiDischargeDraft = body.AllowAiDischargeDraft,
                AllowAiMentalHealthNotes = body.AllowAiMentalHealthNotes
            },
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>List patients with access to this hospital.</summary>
    [HttpGet("{id:guid}/patients", Name = "GetAdminClinicPatients")]
    [ProducesResponseType(typeof(PagedResult<AdminClinicPatientListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPatients(
        Guid id,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new GetAdminClinicPatientsQuery { ClinicId = id, PageNumber = pageNumber, PageSize = pageSize, Search = search },
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Get one patient linked to this hospital (summary and recent visits).</summary>
    [HttpGet("{id:guid}/patients/{patientId:guid}", Name = "GetAdminClinicPatientById")]
    [ProducesResponseType(typeof(AdminClinicPatientDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPatientById(
        Guid id,
        Guid patientId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetAdminClinicPatientByIdQuery { ClinicId = id, PatientId = patientId },
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Private profile photo for a patient linked to this hospital.</summary>
    [HttpGet("{id:guid}/patients/{patientId:guid}/photo", Name = "GetAdminClinicPatientPhoto")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPatientPhoto(
        Guid id,
        Guid patientId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetAdminClinicPatientPhotoQuery { ClinicId = id, PatientId = patientId },
            cancellationToken);
        return result is null ? NotFound() : File(result.Content, result.ContentType);
    }

    /// <summary>Grant an existing patient access to this hospital by email.</summary>
    [HttpPost("{id:guid}/patients", Name = "GrantAdminClinicPatientAccess")]
    [ProducesResponseType(typeof(AdminClinicPatientListItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GrantPatientAccess(
        Guid id,
        [FromBody] GrantAdminClinicPatientAccessRequest body,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GrantAdminClinicPatientAccessCommand
            {
                ClinicId = id,
                Email = body.Email,
                Notes = body.Notes
            },
            cancellationToken);
        return result is null
            ? NotFound()
            : CreatedAtAction(nameof(GetPatientById), new { id, patientId = result.PatientId }, result);
    }

    /// <summary>Revoke a patient's access to this hospital.</summary>
    [HttpDelete("{id:guid}/patients/{patientId:guid}", Name = "RevokeAdminClinicPatientAccess")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokePatientAccess(Guid id, Guid patientId, CancellationToken cancellationToken)
    {
        var revoked = await mediator.Send(
            new RevokeAdminClinicPatientAccessCommand { ClinicId = id, PatientId = patientId },
            cancellationToken);
        return revoked ? NoContent() : NotFound();
    }

    /// <summary>Link the current user to a hospital (idempotent).</summary>
    [HttpPost("{id:guid}/membership", Name = "EnsureClinicMembership")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> EnsureMembership(Guid id, CancellationToken cancellationToken)
    {
        var linked = await mediator.Send(new EnsureClinicMembershipCommand { ClinicId = id }, cancellationToken);
        return linked ? NoContent() : Forbid();
    }

    /// <summary>Link the current user to an existing hospital by RaphCare reference or registration number (first claim only).</summary>
    [HttpPost("claim", Name = "ClaimClinicByRegistrationNumber")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ClaimByRegistrationNumber(
        [FromBody] ClaimClinicByRegistrationNumberRequest body,
        CancellationToken cancellationToken)
    {
        var clinicId = await mediator.Send(
            new ClaimClinicByRegistrationNumberCommand { RegistrationNumber = body.RegistrationNumber },
            cancellationToken);
        return clinicId is null ? NotFound() : Ok(new { clinicId });
    }

    /// <summary>Onboard a new hospital with optional primary facility.</summary>
    [HttpPost(Name = "RegisterClinic")]
    [ProducesResponseType(typeof(RegisterClinicResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterClinicCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetList), new { id = result.ClinicId }, result);
        }
        catch (InvalidOperationException ex) when (
            ex.Message.Contains("hospital reference", StringComparison.OrdinalIgnoreCase)
            || ex.Message.Contains("registration number", StringComparison.OrdinalIgnoreCase))
        {
            return Conflict(new { error = ex.Message });
        }
    }

    /// <summary>List staff linked to a hospital.</summary>
    [HttpGet("{id:guid}/staff", Name = "GetAdminClinicStaff")]
    [ProducesResponseType(typeof(IReadOnlyList<ClinicStaffMemberDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStaff(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAdminClinicStaffQuery { ClinicId = id }, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Invite a healthcare professional to the hospital by email.</summary>
    [HttpPost("{id:guid}/staff", Name = "InviteClinicStaff")]
    [ProducesResponseType(typeof(ClinicStaffMemberDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> InviteStaff(
        Guid id,
        [FromBody] InviteClinicStaffRequest body,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new InviteClinicStaffCommand { ClinicId = id, Email = body.Email, JobRole = body.JobRole ?? string.Empty },
            cancellationToken);
        return CreatedAtAction(nameof(GetStaff), new { id }, result);
    }

    /// <summary>Promote or demote a staff member's administrator role for the clinic portal.</summary>
    [HttpPut("{id:guid}/staff/{userId:guid}/role", Name = "UpdateClinicStaffRole")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStaffRole(
        Guid id,
        Guid userId,
        [FromBody] UpdateClinicStaffRoleRequest body,
        CancellationToken cancellationToken)
    {
        var updated = await mediator.Send(
            new UpdateClinicStaffRoleCommand
            {
                ClinicId = id,
                UserId = userId,
                IsAdministrator = body.IsAdministrator,
                JobRole = body.JobRole
            },
            cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    /// <summary>Resend the clinic portal invitation email to a staff member who has not signed in yet.</summary>
    [HttpPost("{id:guid}/staff/{userId:guid}/resend-invitation", Name = "ResendClinicStaffInvitation")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResendStaffInvitation(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        var sent = await mediator.Send(
            new ResendClinicStaffInvitationCommand { ClinicId = id, UserId = userId },
            cancellationToken);
        return sent ? NoContent() : NotFound();
    }

    /// <summary>Resend a pending staff invitation email to someone who has not registered yet.</summary>
    [HttpPost("{id:guid}/staff/invitations/{invitationId:guid}/resend", Name = "ResendPendingStaffInvitation")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResendPendingInvitation(
        Guid id,
        Guid invitationId,
        CancellationToken cancellationToken)
    {
        var sent = await mediator.Send(
            new ResendPendingStaffInvitationCommand { ClinicId = id, InvitationId = invitationId },
            cancellationToken);
        return sent ? NoContent() : NotFound();
    }

    /// <summary>Cancel a pending staff invitation.</summary>
    [HttpDelete("{id:guid}/staff/invitations/{invitationId:guid}", Name = "CancelPendingStaffInvitation")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelPendingInvitation(
        Guid id,
        Guid invitationId,
        CancellationToken cancellationToken)
    {
        var cancelled = await mediator.Send(
            new CancelPendingStaffInvitationCommand { ClinicId = id, InvitationId = invitationId },
            cancellationToken);
        return cancelled ? NoContent() : NotFound();
    }

    /// <summary>Remove a staff member from the hospital.</summary>
    [HttpDelete("{id:guid}/staff/{userId:guid}", Name = "RemoveClinicStaff")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveStaff(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        var removed = await mediator.Send(
            new RemoveClinicStaffCommand { ClinicId = id, UserId = userId },
            cancellationToken);
        return removed ? NoContent() : NotFound();
    }

    /// <summary>Add a facility to a hospital.</summary>
    [HttpPost("{id:guid}/facilities", Name = "CreateAdminFacility")]
    [ProducesResponseType(typeof(FacilityListItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateFacility(
        Guid id,
        [FromBody] CreateAdminFacilityRequest body,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateAdminFacilityCommand
            {
                ClinicId = id,
                Name = body.Name,
                Address = body.Address,
                City = body.City,
                Country = body.Country,
                IsVirtual = body.IsVirtual
            },
            cancellationToken);
        return result is null ? NotFound() : CreatedAtAction(nameof(GetById), new { id }, result);
    }

    /// <summary>Update a hospital facility.</summary>
    [HttpPut("{id:guid}/facilities/{facilityId:guid}", Name = "UpdateAdminFacility")]
    [ProducesResponseType(typeof(FacilityListItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateFacility(
        Guid id,
        Guid facilityId,
        [FromBody] UpdateAdminFacilityRequest body,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateAdminFacilityCommand
            {
                ClinicId = id,
                FacilityId = facilityId,
                Name = body.Name,
                Address = body.Address,
                City = body.City,
                Country = body.Country,
                IsVirtual = body.IsVirtual
            },
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Delete a hospital facility.</summary>
    [HttpDelete("{id:guid}/facilities/{facilityId:guid}", Name = "DeleteAdminFacility")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteFacility(Guid id, Guid facilityId, CancellationToken cancellationToken)
    {
        var deleted = await mediator.Send(
            new DeleteAdminFacilityCommand { ClinicId = id, FacilityId = facilityId },
            cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    /// <summary>Dashboard metrics scoped to this hospital.</summary>
    [HttpGet("{id:guid}/dashboard", Name = "GetAdminClinicDashboard")]
    [ProducesResponseType(typeof(AdminClinicDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDashboard(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAdminClinicDashboardQuery { ClinicId = id }, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>List providers linked to this hospital.</summary>
    [HttpGet("{id:guid}/providers", Name = "GetAdminClinicProviders")]
    [ProducesResponseType(typeof(IReadOnlyList<AdminClinicProviderListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProviders(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAdminClinicProvidersQuery { ClinicId = id }, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Get one provider with weekly schedule.</summary>
    [HttpGet("{id:guid}/providers/{providerId:guid}", Name = "GetAdminClinicProviderById")]
    [ProducesResponseType(typeof(AdminClinicProviderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProviderById(Guid id, Guid providerId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetAdminClinicProviderByIdQuery { ClinicId = id, ProviderId = providerId },
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Link a staff member as a clinical provider for this hospital.</summary>
    [HttpPost("{id:guid}/providers", Name = "CreateAdminClinicProvider")]
    [ProducesResponseType(typeof(AdminClinicProviderListItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateProvider(
        Guid id,
        [FromBody] CreateAdminClinicProviderRequest body,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateAdminClinicProviderCommand
            {
                ClinicId = id,
                UserId = body.UserId,
                LicenseNumber = body.LicenseNumber
            },
            cancellationToken);
        return result is null
            ? NotFound()
            : CreatedAtAction(nameof(GetProviderById), new { id, providerId = result.ProviderId }, result);
    }

    /// <summary>Activate or deactivate a clinical provider for this hospital.</summary>
    [HttpPost("{id:guid}/providers/{providerId:guid}/active", Name = "SetAdminClinicProviderActive")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetProviderActive(
        Guid id,
        Guid providerId,
        [FromBody] SetAdminClinicProviderActiveRequest body,
        CancellationToken cancellationToken)
    {
        var updated = await mediator.Send(
            new SetAdminClinicProviderActiveCommand
            {
                ClinicId = id,
                ProviderId = providerId,
                IsActive = body.IsActive
            },
            cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    /// <summary>Add a recurring weekly schedule slot for a provider.</summary>
    [HttpPost("{id:guid}/providers/{providerId:guid}/schedules", Name = "CreateAdminClinicProviderSchedule")]
    [ProducesResponseType(typeof(AdminClinicProviderScheduleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateProviderSchedule(
        Guid id,
        Guid providerId,
        [FromBody] CreateAdminClinicProviderScheduleRequest body,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateAdminClinicProviderScheduleCommand
            {
                ClinicId = id,
                ProviderId = providerId,
                Day = body.Day,
                StartTime = body.StartTime,
                EndTime = body.EndTime
            },
            cancellationToken);
        return result is null ? NotFound() : CreatedAtAction(nameof(GetProviderById), new { id, providerId }, result);
    }

    /// <summary>Remove a provider schedule slot.</summary>
    [HttpDelete("{id:guid}/providers/{providerId:guid}/schedules/{scheduleId:guid}", Name = "DeleteAdminClinicProviderSchedule")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProviderSchedule(
        Guid id,
        Guid providerId,
        Guid scheduleId,
        CancellationToken cancellationToken)
    {
        var deleted = await mediator.Send(
            new DeleteAdminClinicProviderScheduleCommand
            {
                ClinicId = id,
                ProviderId = providerId,
                ScheduleId = scheduleId
            },
            cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    /// <summary>List appointments for this hospital.</summary>
    [HttpGet("{id:guid}/appointments", Name = "GetAdminClinicAppointments")]
    [ProducesResponseType(typeof(PagedResult<AdminClinicAppointmentListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAppointments(
        Guid id,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] DateTime? fromUtc = null,
        [FromQuery] DateTime? toUtc = null,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new GetAdminClinicAppointmentsQuery
            {
                ClinicId = id,
                PageNumber = pageNumber,
                PageSize = pageSize,
                FromUtc = fromUtc,
                ToUtc = toUtc,
                Status = status
            },
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Book an appointment for a patient at this hospital.</summary>
    [HttpPost("{id:guid}/appointments", Name = "CreateAdminClinicAppointment")]
    [ProducesResponseType(typeof(AdminClinicAppointmentListItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAppointment(
        Guid id,
        [FromBody] CreateAdminClinicAppointmentRequest body,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateAdminClinicAppointmentCommand
            {
                ClinicId = id,
                PatientId = body.PatientId,
                ProviderId = body.ProviderId,
                ScheduledStart = body.ScheduledStart,
                ScheduledEnd = body.ScheduledEnd,
                Type = body.Type,
                Reason = body.Reason
            },
            cancellationToken);
        return result is null
            ? NotFound()
            : CreatedAtAction(nameof(GetAppointments), new { id }, result);
    }

    /// <summary>Cancel an appointment at this hospital.</summary>
    [HttpDelete("{id:guid}/appointments/{appointmentId:guid}", Name = "CancelAdminClinicAppointment")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelAppointment(
        Guid id,
        Guid appointmentId,
        CancellationToken cancellationToken)
    {
        var cancelled = await mediator.Send(
            new CancelAdminClinicAppointmentCommand { ClinicId = id, AppointmentId = appointmentId },
            cancellationToken);
        return cancelled ? NoContent() : NotFound();
    }

    /// <summary>Reschedule an appointment at this hospital.</summary>
    [HttpPut("{id:guid}/appointments/{appointmentId:guid}", Name = "RescheduleAdminClinicAppointment")]
    [ProducesResponseType(typeof(AdminClinicAppointmentListItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RescheduleAppointment(
        Guid id,
        Guid appointmentId,
        [FromBody] RescheduleAdminClinicAppointmentRequest body,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new RescheduleAdminClinicAppointmentCommand
            {
                ClinicId = id,
                AppointmentId = appointmentId,
                ProviderId = body.ProviderId,
                ScheduledStart = body.ScheduledStart,
                ScheduledEnd = body.ScheduledEnd,
                Reason = body.Reason
            },
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Start a clinical visit from an appointment.</summary>
    [HttpPost("{id:guid}/appointments/{appointmentId:guid}/visit", Name = "StartAdminClinicVisit")]
    [ProducesResponseType(typeof(AdminClinicVisitDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StartVisit(
        Guid id,
        Guid appointmentId,
        [FromBody] StartAdminClinicVisitRequest? body,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new StartAdminClinicVisitCommand
            {
                ClinicId = id,
                AppointmentId = appointmentId,
                Summary = body?.Summary
            },
            cancellationToken);
        return result is null
            ? NotFound()
            : CreatedAtAction(nameof(GetVisitById), new { id, visitId = result.Id }, result);
    }

    /// <summary>Get a clinical visit with vitals.</summary>
    [HttpGet("{id:guid}/visits/{visitId:guid}", Name = "GetAdminClinicVisitById")]
    [ProducesResponseType(typeof(AdminClinicVisitDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVisitById(Guid id, Guid visitId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetAdminClinicVisitByIdQuery { ClinicId = id, VisitId = visitId },
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Complete a clinical visit.</summary>
    [HttpPost("{id:guid}/visits/{visitId:guid}/complete", Name = "CompleteAdminClinicVisit")]
    [ProducesResponseType(typeof(AdminClinicVisitDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteVisit(
        Guid id,
        Guid visitId,
        [FromBody] CompleteAdminClinicVisitRequest? body,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CompleteAdminClinicVisitCommand
            {
                ClinicId = id,
                VisitId = visitId,
                Summary = body?.Summary
            },
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Record a vital sign on a visit.</summary>
    [HttpPost("{id:guid}/visits/{visitId:guid}/vitals", Name = "CreateAdminClinicVisitVital")]
    [ProducesResponseType(typeof(AdminClinicVisitVitalDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateVisitVital(
        Guid id,
        Guid visitId,
        [FromBody] CreateAdminClinicVisitVitalRequest body,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateAdminClinicVisitVitalCommand
            {
                ClinicId = id,
                VisitId = visitId,
                Type = body.Type,
                Value = body.Value,
                Unit = body.Unit,
                RecordedAt = body.RecordedAt
            },
            cancellationToken);
        return result is null
            ? NotFound()
            : CreatedAtAction(nameof(GetVisitById), new { id, visitId }, result);
    }

    /// <summary>Save or update the SOAP note for a visit.</summary>
    [HttpPut("{id:guid}/visits/{visitId:guid}/soap", Name = "UpsertAdminClinicVisitSoapNote")]
    [ProducesResponseType(typeof(AdminClinicVisitSoapNoteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpsertVisitSoapNote(
        Guid id,
        Guid visitId,
        [FromBody] UpsertAdminClinicVisitSoapNoteRequest body,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpsertAdminClinicVisitSoapNoteCommand
            {
                ClinicId = id,
                VisitId = visitId,
                Subjective = body.Subjective,
                Objective = body.Objective,
                Assessment = body.Assessment,
                Plan = body.Plan
            },
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Add a free-text clinical note to a visit.</summary>
    [HttpPost("{id:guid}/visits/{visitId:guid}/notes", Name = "CreateAdminClinicVisitNote")]
    [ProducesResponseType(typeof(AdminClinicVisitNoteDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateVisitNote(
        Guid id,
        Guid visitId,
        [FromBody] CreateAdminClinicVisitNoteRequest body,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateAdminClinicVisitNoteCommand
            {
                ClinicId = id,
                VisitId = visitId,
                Notes = body.Notes,
                Category = body.Category
            },
            cancellationToken);
        return result is null
            ? NotFound()
            : CreatedAtAction(nameof(GetVisitById), new { id, visitId }, result);
    }

    /// <summary>Add a prescription with one or more medication lines.</summary>
    [HttpPost("{id:guid}/visits/{visitId:guid}/prescriptions", Name = "CreateAdminClinicVisitPrescription")]
    [ProducesResponseType(typeof(AdminClinicVisitPrescriptionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateVisitPrescription(
        Guid id,
        Guid visitId,
        [FromBody] CreateAdminClinicVisitPrescriptionRequest body,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateAdminClinicVisitPrescriptionCommand
            {
                ClinicId = id,
                VisitId = visitId,
                MedicationName = body.MedicationName,
                Dosage = body.Dosage,
                Frequency = body.Frequency,
                DurationDays = body.DurationDays,
                Notes = body.Notes,
                Items = (body.Items ?? Array.Empty<CreateAdminClinicVisitPrescriptionItemRequest>())
                    .Select(i => new CreateAdminClinicVisitPrescriptionLine
                    {
                        MedicationName = i.MedicationName,
                        Dosage = i.Dosage,
                        Frequency = i.Frequency,
                        DurationDays = i.DurationDays
                    })
                    .ToList()
            },
            cancellationToken);
        return result is null
            ? NotFound()
            : CreatedAtAction(nameof(GetVisitById), new { id, visitId }, result);
    }

    /// <summary>Order a lab test for a visit. Staff record the result later at collection.</summary>
    [HttpPost("{id:guid}/visits/{visitId:guid}/lab-orders", Name = "CreateAdminClinicVisitLabOrder")]
    [ProducesResponseType(typeof(AdminClinicVisitLabResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateVisitLabOrder(
        Guid id,
        Guid visitId,
        [FromBody] CreateAdminClinicVisitLabResultRequest body,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateAdminClinicVisitLabResultCommand
            {
                ClinicId = id,
                VisitId = visitId,
                TestName = body.TestName,
                Priority = body.Priority
            },
            cancellationToken);
        return result is null
            ? NotFound()
            : CreatedAtAction(nameof(GetVisitById), new { id, visitId }, result);
    }

    /// <summary>Record a result for a pending lab order. Any hospital staff can complete this.</summary>
    [HttpPost("{id:guid}/lab-orders/{labRequestId:guid}/results", Name = "CompleteAdminClinicLabOrder")]
    [ProducesResponseType(typeof(AdminClinicVisitLabResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteLabOrder(
        Guid id,
        Guid labRequestId,
        [FromBody] CompleteAdminClinicLabOrderRequest body,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CompleteAdminClinicLabOrderCommand
            {
                ClinicId = id,
                LabRequestId = labRequestId,
                ResultValue = body.ResultValue,
                Unit = body.Unit,
                ReferenceRange = body.ReferenceRange
            },
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Mark a prescription as collected. Any hospital staff can complete this.</summary>
    [HttpPost("{id:guid}/prescriptions/{prescriptionId:guid}/dispense", Name = "DispenseAdminClinicPrescription")]
    [ProducesResponseType(typeof(AdminClinicVisitPrescriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DispensePrescription(
        Guid id,
        Guid prescriptionId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new DispenseAdminClinicPrescriptionCommand
            {
                ClinicId = id,
                PrescriptionId = prescriptionId
            },
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Cancel a waiting prescription. Any hospital staff can do this.</summary>
    [HttpPost("{id:guid}/prescriptions/{prescriptionId:guid}/cancel", Name = "CancelAdminClinicPrescription")]
    [ProducesResponseType(typeof(AdminClinicVisitPrescriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelPrescription(
        Guid id,
        Guid prescriptionId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CancelAdminClinicPrescriptionCommand
            {
                ClinicId = id,
                PrescriptionId = prescriptionId
            },
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Put a collected or cancelled prescription back to waiting.</summary>
    [HttpPost("{id:guid}/prescriptions/{prescriptionId:guid}/undo", Name = "UndoAdminClinicPrescription")]
    [ProducesResponseType(typeof(AdminClinicVisitPrescriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UndoPrescription(
        Guid id,
        Guid prescriptionId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UndoAdminClinicPrescriptionCommand
            {
                ClinicId = id,
                PrescriptionId = prescriptionId
            },
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Cancel a waiting lab order. Any hospital staff can do this.</summary>
    [HttpPost("{id:guid}/lab-orders/{labRequestId:guid}/cancel", Name = "CancelAdminClinicLabOrder")]
    [ProducesResponseType(typeof(AdminClinicVisitLabResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelLabOrder(
        Guid id,
        Guid labRequestId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CancelAdminClinicLabOrderCommand
            {
                ClinicId = id,
                LabRequestId = labRequestId
            },
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Put a completed or cancelled lab order back to waiting.</summary>
    [HttpPost("{id:guid}/lab-orders/{labRequestId:guid}/undo", Name = "UndoAdminClinicLabOrder")]
    [ProducesResponseType(typeof(AdminClinicVisitLabResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UndoLabOrder(
        Guid id,
        Guid labRequestId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UndoAdminClinicLabOrderCommand
            {
                ClinicId = id,
                LabRequestId = labRequestId
            },
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Call a waiting prescription so its pickup code shows on the waiting screen.</summary>
    [HttpPost("{id:guid}/prescriptions/{prescriptionId:guid}/call", Name = "CallAdminClinicPrescription")]
    [ProducesResponseType(typeof(AdminClinicVisitPrescriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CallPrescription(
        Guid id,
        Guid prescriptionId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CallAdminClinicPrescriptionCommand
            {
                ClinicId = id,
                PrescriptionId = prescriptionId
            },
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Call a waiting lab order so its pickup code shows on the waiting screen.</summary>
    [HttpPost("{id:guid}/lab-orders/{labRequestId:guid}/call", Name = "CallAdminClinicLabOrder")]
    [ProducesResponseType(typeof(AdminClinicVisitLabResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CallLabOrder(
        Guid id,
        Guid labRequestId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CallAdminClinicLabOrderCommand
            {
                ClinicId = id,
                LabRequestId = labRequestId
            },
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Pending prescriptions and lab orders waiting at this hospital.</summary>
    [HttpGet("{id:guid}/collection-orders", Name = "GetAdminClinicCollectionOrders")]
    [ProducesResponseType(typeof(AdminClinicCollectionBoardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCollectionOrders(
        Guid id,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetAdminClinicCollectionOrdersQuery { ClinicId = id, Search = search },
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Create or return the waiting-screen token for this hospital.</summary>
    [HttpPost("{id:guid}/collection-display", Name = "EnsureAdminClinicCollectionDisplay")]
    [ProducesResponseType(typeof(CollectionDisplayLinkDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EnsureCollectionDisplay(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new EnsureCollectionDisplayTokenCommand { ClinicId = id },
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Casualty / triage queue board for this hospital.</summary>
    [HttpGet("{id:guid}/casualty/board", Name = "GetAdminClinicCasualtyBoard")]
    [ProducesResponseType(typeof(AdminClinicCasualtyBoardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCasualtyBoard(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetAdminClinicCasualtyBoardQuery { ClinicId = id },
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Enqueue a walk-in on the casualty board.</summary>
    [HttpPost("{id:guid}/casualty/tickets", Name = "CreateAdminClinicCasualtyTicket")]
    [ProducesResponseType(typeof(AdminClinicCasualtyTicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateCasualtyTicket(
        Guid id,
        [FromBody] CreateAdminClinicCasualtyTicketRequest? body,
        CancellationToken cancellationToken)
    {
        body ??= new CreateAdminClinicCasualtyTicketRequest();
        var result = await mediator.Send(
            new CreateAdminClinicCasualtyTicketCommand
            {
                ClinicId = id,
                PatientId = body.PatientId,
                TriageLevel = body.TriageLevel,
                ChiefComplaint = body.ChiefComplaint
            },
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Call a casualty queue code onto the waiting screen.</summary>
    [HttpPost("{id:guid}/casualty/tickets/{ticketId:guid}/call", Name = "CallAdminClinicCasualtyTicket")]
    [ProducesResponseType(typeof(AdminClinicCasualtyTicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CallCasualtyTicket(
        Guid id,
        Guid ticketId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CallAdminClinicCasualtyTicketCommand { ClinicId = id, TicketId = ticketId },
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Complete or cancel a casualty ticket.</summary>
    [HttpPost("{id:guid}/casualty/tickets/{ticketId:guid}/complete", Name = "CompleteAdminClinicCasualtyTicket")]
    [ProducesResponseType(typeof(AdminClinicCasualtyTicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteCasualtyTicket(
        Guid id,
        Guid ticketId,
        [FromBody] CompleteAdminClinicCasualtyTicketRequest? body,
        CancellationToken cancellationToken)
    {
        body ??= new CompleteAdminClinicCasualtyTicketRequest();
        var result = await mediator.Send(
            new CompleteAdminClinicCasualtyTicketCommand
            {
                ClinicId = id,
                TicketId = ticketId,
                Cancel = body.Cancel
            },
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Create or return the casualty waiting-screen token for this hospital.</summary>
    [HttpPost("{id:guid}/casualty-display", Name = "EnsureAdminClinicCasualtyDisplay")]
    [ProducesResponseType(typeof(CasualtyDisplayLinkDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EnsureCasualtyDisplay(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new EnsureCasualtyDisplayTokenCommand { ClinicId = id },
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Theatre board for a calendar day (UTC). Defaults to today.</summary>
    [HttpGet("{id:guid}/theatre/board", Name = "GetAdminClinicTheatreBoard")]
    [ProducesResponseType(typeof(AdminClinicTheatreBoardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTheatreBoard(
        Guid id,
        [FromQuery] DateTime? dayUtc,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetAdminClinicTheatreBoardQuery { ClinicId = id, DayUtc = dayUtc },
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Schedule a theatre case.</summary>
    [HttpPost("{id:guid}/theatre/cases", Name = "CreateAdminClinicTheatreCase")]
    [ProducesResponseType(typeof(AdminClinicTheatreCaseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateTheatreCase(
        Guid id,
        [FromBody] CreateAdminClinicTheatreCaseRequest? body,
        CancellationToken cancellationToken)
    {
        body ??= new CreateAdminClinicTheatreCaseRequest();
        var result = await mediator.Send(
            new CreateAdminClinicTheatreCaseCommand
            {
                ClinicId = id,
                PatientId = body.PatientId,
                ScheduledStart = body.ScheduledStart,
                ScheduledEnd = body.ScheduledEnd,
                ProcedureName = body.ProcedureName,
                TheatreName = body.TheatreName,
                SurgeonName = body.SurgeonName,
                Notes = body.Notes
            },
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Update theatre case status (start, complete, or cancel).</summary>
    [HttpPost("{id:guid}/theatre/cases/{caseId:guid}/status", Name = "UpdateAdminClinicTheatreCaseStatus")]
    [ProducesResponseType(typeof(AdminClinicTheatreCaseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTheatreCaseStatus(
        Guid id,
        Guid caseId,
        [FromBody] UpdateAdminClinicTheatreCaseStatusRequest? body,
        CancellationToken cancellationToken)
    {
        body ??= new UpdateAdminClinicTheatreCaseStatusRequest();
        var result = await mediator.Send(
            new UpdateAdminClinicTheatreCaseStatusCommand
            {
                ClinicId = id,
                CaseId = caseId,
                Status = body.Status
            },
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Outbound referral board for this hospital.</summary>
    [HttpGet("{id:guid}/referrals/board", Name = "GetAdminClinicReferralBoard")]
    [ProducesResponseType(typeof(AdminClinicReferralBoardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReferralBoard(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetAdminClinicReferralBoardQuery { ClinicId = id },
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Log an outbound referral.</summary>
    [HttpPost("{id:guid}/referrals", Name = "CreateAdminClinicReferral")]
    [ProducesResponseType(typeof(AdminClinicReferralDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateReferral(
        Guid id,
        [FromBody] CreateAdminClinicReferralRequest? body,
        CancellationToken cancellationToken)
    {
        body ??= new CreateAdminClinicReferralRequest();
        var result = await mediator.Send(
            new CreateAdminClinicReferralCommand
            {
                ClinicId = id,
                PatientId = body.PatientId,
                VisitId = body.VisitId,
                ReferredTo = body.ReferredTo,
                Reason = body.Reason,
                Specialty = body.Specialty,
                Notes = body.Notes
            },
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Update referral status (accept, complete, or cancel).</summary>
    [HttpPost("{id:guid}/referrals/{referralId:guid}/status", Name = "UpdateAdminClinicReferralStatus")]
    [ProducesResponseType(typeof(AdminClinicReferralDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateReferralStatus(
        Guid id,
        Guid referralId,
        [FromBody] UpdateAdminClinicReferralStatusRequest? body,
        CancellationToken cancellationToken)
    {
        body ??= new UpdateAdminClinicReferralStatusRequest();
        var result = await mediator.Send(
            new UpdateAdminClinicReferralStatusCommand
            {
                ClinicId = id,
                ReferralId = referralId,
                Status = body.Status
            },
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Staff roster board for a calendar day (UTC). Defaults to today.</summary>
    [HttpGet("{id:guid}/roster/board", Name = "GetAdminClinicRosterBoard")]
    [ProducesResponseType(typeof(AdminClinicRosterBoardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRosterBoard(
        Guid id,
        [FromQuery] DateTime? dayUtc,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetAdminClinicRosterBoardQuery { ClinicId = id, DayUtc = dayUtc },
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Add a staff member to the roster for a shift.</summary>
    [HttpPost("{id:guid}/roster/entries", Name = "CreateAdminClinicRosterEntry")]
    [ProducesResponseType(typeof(AdminClinicRosterEntryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateRosterEntry(
        Guid id,
        [FromBody] CreateAdminClinicRosterEntryRequest? body,
        CancellationToken cancellationToken)
    {
        body ??= new CreateAdminClinicRosterEntryRequest();
        var result = await mediator.Send(
            new CreateAdminClinicRosterEntryCommand
            {
                ClinicId = id,
                ApplicationUserId = body.ApplicationUserId,
                DutyDate = body.DutyDate,
                ShiftLabel = body.ShiftLabel,
                Note = body.Note
            },
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Remove a roster entry.</summary>
    [HttpDelete("{id:guid}/roster/entries/{entryId:guid}", Name = "DeleteAdminClinicRosterEntry")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRosterEntry(
        Guid id,
        Guid entryId,
        CancellationToken cancellationToken)
    {
        var deleted = await mediator.Send(
            new DeleteAdminClinicRosterEntryCommand { ClinicId = id, EntryId = entryId },
            cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    /// <summary>List monitoring devices registered to this hospital.</summary>
    [HttpGet("{id:guid}/devices", Name = "GetAdminClinicDevices")]
    [ProducesResponseType(typeof(IReadOnlyList<AdminClinicDeviceListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDevices(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAdminClinicDevicesQuery { ClinicId = id }, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Assign an in-stock hospital device to a patient of this hospital.</summary>
    [HttpPost("{id:guid}/devices/{deviceId:guid}/assign", Name = "AssignAdminClinicDeviceToPatient")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignDeviceToPatient(
        Guid id,
        Guid deviceId,
        [FromBody] AssignAdminClinicDeviceToPatientRequest body,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new AssignAdminClinicDeviceToPatientCommand
            {
                ClinicId = id,
                DeviceId = deviceId,
                PatientId = body.PatientId
            },
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Inpatient board: wards, rooms, beds, and active admissions.</summary>
    [HttpGet("{id:guid}/inpatient/board", Name = "GetAdminClinicInpatientBoard")]
    [ProducesResponseType(typeof(AdminClinicInpatientBoardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInpatientBoard(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAdminClinicInpatientBoardQuery { ClinicId = id }, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Create a ward under a facility.</summary>
    [HttpPost("{id:guid}/wards", Name = "CreateAdminClinicWard")]
    [ProducesResponseType(typeof(AdminClinicWardDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateWard(
        Guid id,
        [FromBody] CreateAdminClinicWardRequest body,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateAdminClinicWardCommand
            {
                ClinicId = id,
                FacilityId = body.FacilityId,
                Name = body.Name,
                Code = body.Code
            },
            cancellationToken);
        return result is null ? NotFound() : CreatedAtAction(nameof(GetInpatientBoard), new { id }, result);
    }

    /// <summary>Create a room under a ward.</summary>
    [HttpPost("{id:guid}/rooms", Name = "CreateAdminClinicRoom")]
    [ProducesResponseType(typeof(AdminClinicRoomDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateRoom(
        Guid id,
        [FromBody] CreateAdminClinicRoomRequest body,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateAdminClinicRoomCommand
            {
                ClinicId = id,
                WardId = body.WardId,
                Name = body.Name,
                RoomType = body.RoomType
            },
            cancellationToken);
        return result is null ? NotFound() : CreatedAtAction(nameof(GetInpatientBoard), new { id }, result);
    }

    /// <summary>Create a bed under a room.</summary>
    [HttpPost("{id:guid}/beds", Name = "CreateAdminClinicBed")]
    [ProducesResponseType(typeof(AdminClinicBedDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateBed(
        Guid id,
        [FromBody] CreateAdminClinicBedRequest body,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateAdminClinicBedCommand
            {
                ClinicId = id,
                RoomId = body.RoomId,
                Label = body.Label
            },
            cancellationToken);
        return result is null ? NotFound() : CreatedAtAction(nameof(GetInpatientBoard), new { id }, result);
    }

    /// <summary>Admit a patient to a bed.</summary>
    [HttpPost("{id:guid}/admissions", Name = "AdmitAdminClinicPatient")]
    [ProducesResponseType(typeof(AdminClinicAdmissionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AdmitPatient(
        Guid id,
        [FromBody] AdmitAdminClinicPatientRequest body,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new AdmitAdminClinicPatientCommand
            {
                ClinicId = id,
                PatientId = body.PatientId,
                BedId = body.BedId,
                Reason = body.Reason,
                Notes = body.Notes
            },
            cancellationToken);
        return result is null ? NotFound() : CreatedAtAction(nameof(GetInpatientBoard), new { id }, result);
    }

    /// <summary>Discharge an inpatient admission.</summary>
    [HttpPost("{id:guid}/admissions/{admissionId:guid}/discharge", Name = "DischargeAdminClinicAdmission")]
    [ProducesResponseType(typeof(AdminClinicAdmissionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DischargeAdmission(
        Guid id,
        Guid admissionId,
        [FromBody] DischargeAdminClinicAdmissionRequest? body,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new DischargeAdminClinicAdmissionCommand
            {
                ClinicId = id,
                AdmissionId = admissionId,
                Notes = body?.Notes,
                DischargeSummary = body?.DischargeSummary,
                NightlyBedRate = body?.NightlyBedRate,
                ExtraAmount = body?.ExtraAmount,
                ExtraDescription = body?.ExtraDescription,
                MarkPaid = body?.MarkPaid ?? false,
                Currency = body?.Currency,
                BookReturnVisit = body?.BookReturnVisit ?? false,
                ReturnProviderId = body?.ReturnProviderId,
                ReturnScheduledStart = body?.ReturnScheduledStart,
                ReturnScheduledEnd = body?.ReturnScheduledEnd,
                ReturnAppointmentType = string.IsNullOrWhiteSpace(body?.ReturnAppointmentType)
                    ? "InPerson"
                    : body.ReturnAppointmentType,
                ReturnReason = body?.ReturnReason
            },
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Draft a discharge summary with AI from ward notes (staff edits before discharge).</summary>
    [HttpPost("{id:guid}/admissions/{admissionId:guid}/discharge-summary/draft", Name = "DraftAdminClinicDischargeSummary")]
    [ProducesResponseType(typeof(AdminClinicDischargeSummaryDraftDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DraftDischargeSummary(
        Guid id,
        Guid admissionId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new DraftAdminClinicDischargeSummaryCommand
            {
                ClinicId = id,
                AdmissionId = admissionId
            },
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>List ward notes for an admission.</summary>
    [HttpGet("{id:guid}/admissions/{admissionId:guid}/observations", Name = "GetAdminClinicAdmissionObservations")]
    [ProducesResponseType(typeof(IReadOnlyList<AdminClinicAdmissionObservationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAdmissionObservations(
        Guid id,
        Guid admissionId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetAdminClinicAdmissionObservationsQuery
            {
                ClinicId = id,
                AdmissionId = admissionId
            },
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Add a ward note or vitals to an active stay.</summary>
    [HttpPost("{id:guid}/admissions/{admissionId:guid}/observations", Name = "CreateAdminClinicAdmissionObservation")]
    [ProducesResponseType(typeof(AdminClinicAdmissionObservationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateAdmissionObservation(
        Guid id,
        Guid admissionId,
        [FromBody] CreateAdminClinicAdmissionObservationRequest body,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateAdminClinicAdmissionObservationCommand
            {
                ClinicId = id,
                AdmissionId = admissionId,
                Note = body.Note,
                HeartRate = body.HeartRate,
                TemperatureCelsius = body.TemperatureCelsius,
                OxygenSaturation = body.OxygenSaturation,
                SystolicBp = body.SystolicBp,
                DiastolicBp = body.DiastolicBp
            },
            cancellationToken);
        return result is null
            ? NotFound()
            : CreatedAtAction(nameof(GetAdmissionObservations), new { id, admissionId }, result);
    }

    /// <summary>Update a ward.</summary>
    [HttpPut("{id:guid}/wards/{wardId:guid}", Name = "UpdateAdminClinicWard")]
    [ProducesResponseType(typeof(AdminClinicWardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateWard(
        Guid id,
        Guid wardId,
        [FromBody] UpdateAdminClinicWardRequest body,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateAdminClinicWardCommand
            {
                ClinicId = id,
                WardId = wardId,
                Name = body.Name,
                Code = body.Code,
                IsActive = body.IsActive
            },
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Delete or archive a ward (archives when admission history exists).</summary>
    [HttpDelete("{id:guid}/wards/{wardId:guid}", Name = "DeleteAdminClinicWard")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteWard(Guid id, Guid wardId, CancellationToken cancellationToken)
    {
        var deleted = await mediator.Send(
            new DeleteAdminClinicWardCommand { ClinicId = id, WardId = wardId },
            cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    /// <summary>Update a room.</summary>
    [HttpPut("{id:guid}/rooms/{roomId:guid}", Name = "UpdateAdminClinicRoom")]
    [ProducesResponseType(typeof(AdminClinicRoomDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRoom(
        Guid id,
        Guid roomId,
        [FromBody] UpdateAdminClinicRoomRequest body,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateAdminClinicRoomCommand
            {
                ClinicId = id,
                RoomId = roomId,
                Name = body.Name,
                RoomType = body.RoomType,
                IsActive = body.IsActive
            },
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Delete or archive a room.</summary>
    [HttpDelete("{id:guid}/rooms/{roomId:guid}", Name = "DeleteAdminClinicRoom")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRoom(Guid id, Guid roomId, CancellationToken cancellationToken)
    {
        var deleted = await mediator.Send(
            new DeleteAdminClinicRoomCommand { ClinicId = id, RoomId = roomId },
            cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    /// <summary>Update a bed label / active flag.</summary>
    [HttpPut("{id:guid}/beds/{bedId:guid}", Name = "UpdateAdminClinicBed")]
    [ProducesResponseType(typeof(AdminClinicBedDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateBed(
        Guid id,
        Guid bedId,
        [FromBody] UpdateAdminClinicBedRequest body,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateAdminClinicBedCommand
            {
                ClinicId = id,
                BedId = bedId,
                Label = body.Label,
                IsActive = body.IsActive
            },
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Set bed status to Available or Maintenance.</summary>
    [HttpPut("{id:guid}/beds/{bedId:guid}/status", Name = "SetAdminClinicBedStatus")]
    [ProducesResponseType(typeof(AdminClinicBedDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetBedStatus(
        Guid id,
        Guid bedId,
        [FromBody] SetAdminClinicBedStatusRequest body,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new SetAdminClinicBedStatusCommand
            {
                ClinicId = id,
                BedId = bedId,
                Status = body.Status
            },
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Delete or archive a bed.</summary>
    [HttpDelete("{id:guid}/beds/{bedId:guid}", Name = "DeleteAdminClinicBed")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBed(Guid id, Guid bedId, CancellationToken cancellationToken)
    {
        var deleted = await mediator.Send(
            new DeleteAdminClinicBedCommand { ClinicId = id, BedId = bedId },
            cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    /// <summary>Transfer an active admission to another available bed.</summary>
    [HttpPost("{id:guid}/admissions/{admissionId:guid}/transfer", Name = "TransferAdminClinicAdmission")]
    [ProducesResponseType(typeof(AdminClinicAdmissionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TransferAdmission(
        Guid id,
        Guid admissionId,
        [FromBody] TransferAdminClinicAdmissionRequest body,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new TransferAdminClinicAdmissionCommand
            {
                ClinicId = id,
                AdmissionId = admissionId,
                TargetBedId = body.TargetBedId,
                Notes = body.Notes
            },
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Paged admission history for a hospital.</summary>
    [HttpGet("{id:guid}/admissions", Name = "GetAdminClinicAdmissions")]
    [ProducesResponseType(typeof(PagedResult<AdminClinicAdmissionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAdmissions(
        Guid id,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new GetAdminClinicAdmissionsQuery
            {
                ClinicId = id,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Status = status
            },
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Get one admission by id.</summary>
    [HttpGet("{id:guid}/admissions/{admissionId:guid}", Name = "GetAdminClinicAdmissionById")]
    [ProducesResponseType(typeof(AdminClinicAdmissionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAdmissionById(Guid id, Guid admissionId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetAdminClinicAdmissionByIdQuery { ClinicId = id, AdmissionId = admissionId },
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Start or resume a telehealth session for a telemedicine appointment (returns Agora join credentials).</summary>
    [HttpPost("{id:guid}/appointments/{appointmentId:guid}/tele-session", Name = "StartAdminClinicTeleSession")]
    [ProducesResponseType(typeof(AdminClinicTeleJoinInfoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StartTeleSession(
        Guid id,
        Guid appointmentId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new StartAdminClinicTeleSessionCommand { ClinicId = id, AppointmentId = appointmentId },
            cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
