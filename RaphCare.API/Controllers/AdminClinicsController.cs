using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Features.Organization.Commands.CancelAdminClinicAppointment;
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
using RaphCare.Application.Features.Organization.Queries.GetAdminClinicById;
using RaphCare.Application.Features.Organization.Queries.GetAdminClinics;
using RaphCare.Application.Features.Organization.Queries.GetAdminClinicStaff;
using RaphCare.Application.Features.Organization.Queries.GetAdminClinicAppointments;
using RaphCare.Application.Features.Organization.Queries.GetAdminClinicDashboard;
using RaphCare.Application.Features.Organization.Queries.GetAdminClinicProviderById;
using RaphCare.Application.Features.Organization.Queries.GetAdminClinicProviders;
using RaphCare.Application.Features.Organization.Queries.GetAdminClinicPatientById;
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
                IsActive = body.IsActive
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

    /// <summary>Link the current user to an existing hospital by registration number (first claim only).</summary>
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
        catch (InvalidOperationException ex) when (ex.Message.Contains("registration number", StringComparison.OrdinalIgnoreCase))
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
            new InviteClinicStaffCommand { ClinicId = id, Email = body.Email },
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
                IsAdministrator = body.IsAdministrator
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
}
