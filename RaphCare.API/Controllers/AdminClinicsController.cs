using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Features.Organization.Commands.CreateAdminFacility;
using RaphCare.Application.Features.Organization.Commands.EnsureClinicMembership;
using RaphCare.Application.Features.Organization.Commands.InviteClinicStaff;
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
                TimeZone = body.TimeZone
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
}
