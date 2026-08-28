using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Clinical;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicVisitVital;

public sealed class CreateAdminClinicVisitVitalHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IUserRoleAssignmentService roleAssignmentService,
    IRepository<Visit> visitRepository,
    IRepository<VitalSignRecord> vitalRepository,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateAdminClinicVisitVitalCommand, AdminClinicVisitVitalDto?>
{
    public async Task<AdminClinicVisitVitalDto?> Handle(
        CreateAdminClinicVisitVitalCommand request,
        CancellationToken cancellationToken)
    {
        if (!await AdminClinicAuthorization.CanDocumentVisitsAsync(
                currentUserService,
                clinicStaffMembershipService,
                roleAssignmentService,
                request.ClinicId,
                cancellationToken)
            .ConfigureAwait(false))
            throw new ForbiddenAccessException("Only a doctor or hospital administrator can record vitals.");

        var visit = await visitRepository.GetByIdAsync(request.VisitId, cancellationToken).ConfigureAwait(false);
        if (visit is null || visit.ClinicId != request.ClinicId)
            return null;

        if (visit.Status is "Completed" or "Cancelled")
            throw new BusinessRuleException("Cannot record vitals on a closed visit.");

        var vital = new VitalSignRecord
        {
            VisitId = visit.Id,
            Type = request.Type.Trim(),
            Value = request.Value,
            Unit = string.IsNullOrWhiteSpace(request.Unit) ? null : request.Unit.Trim(),
            RecordedAt = request.RecordedAt ?? clock.UtcNow
        };

        await vitalRepository.AddAsync(vital, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new AdminClinicVisitVitalDto
        {
            Id = vital.Id,
            Type = vital.Type,
            Value = vital.Value,
            Unit = vital.Unit,
            RecordedAt = vital.RecordedAt
        };
    }
}
