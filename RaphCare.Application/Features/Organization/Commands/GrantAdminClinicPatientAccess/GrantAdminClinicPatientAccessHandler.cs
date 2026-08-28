using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;

namespace RaphCare.Application.Features.Organization.Commands.GrantAdminClinicPatientAccess;

public sealed class GrantAdminClinicPatientAccessHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IUserRoleAssignmentService roleAssignmentService,
    IPatientLookupService patientLookupService,
    IPatientClinicAccessService patientClinicAccessService,
    IAdminClinicPatientQueryService adminClinicPatientQueryService)
    : IRequestHandler<GrantAdminClinicPatientAccessCommand, AdminClinicPatientListItemDto?>
{
    public async Task<AdminClinicPatientListItemDto?> Handle(
        GrantAdminClinicPatientAccessCommand request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.IsClinicAdministratorAsync(
                currentUserService,
                clinicStaffMembershipService,
                roleAssignmentService,
                request.ClinicId,
                cancellationToken)
            .ConfigureAwait(false))
            throw new ForbiddenAccessException("Only hospital administrators can grant patient access.");

        var patient = await patientLookupService
            .FindByEmailAsync(request.Email, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new BusinessRuleException(
                "No patient account found with that email. The patient must register first.");

        await patientClinicAccessService
            .GrantManualAccessAsync(patient.Id, request.ClinicId, request.Notes, cancellationToken)
            .ConfigureAwait(false);

        var detail = await adminClinicPatientQueryService
            .GetPatientDetailAsync(request.ClinicId, patient.Id, cancellationToken)
            .ConfigureAwait(false);

        if (detail is null)
            return null;

        return new AdminClinicPatientListItemDto
        {
            PatientId = detail.PatientId,
            FirstName = detail.FirstName,
            LastName = detail.LastName,
            DateOfBirth = detail.DateOfBirth,
            AccessType = detail.AccessType,
            GrantedAt = detail.GrantedAt
        };
    }
}
