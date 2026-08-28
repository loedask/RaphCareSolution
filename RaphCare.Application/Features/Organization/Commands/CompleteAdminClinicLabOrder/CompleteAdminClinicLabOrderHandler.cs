using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Clinical;

namespace RaphCare.Application.Features.Organization.Commands.CompleteAdminClinicLabOrder;

public sealed class CompleteAdminClinicLabOrderHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IUserRoleAssignmentService roleAssignmentService,
    IRepository<Visit> visitRepository,
    IRepository<LabRequest> labRequestRepository,
    IRepository<LabResult> labResultRepository,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CompleteAdminClinicLabOrderCommand, AdminClinicVisitLabResultDto?>
{
    public async Task<AdminClinicVisitLabResultDto?> Handle(
        CompleteAdminClinicLabOrderCommand request,
        CancellationToken cancellationToken)
    {
        await AdminClinicAuthorization.EnsureCanCompleteLabsAsync(
                currentUserService,
                clinicStaffMembershipService,
                roleAssignmentService,
                request.ClinicId,
                cancellationToken)
            .ConfigureAwait(false);

        var labRequest = await labRequestRepository.GetByIdAsync(request.LabRequestId, cancellationToken).ConfigureAwait(false);
        if (labRequest is null)
            return null;

        var visit = await visitRepository.GetByIdAsync(labRequest.VisitId, cancellationToken).ConfigureAwait(false);
        if (visit is null || visit.ClinicId != request.ClinicId)
            return null;

        if (labRequest.Status is "Completed" or "Cancelled")
            throw new BusinessRuleException(
                labRequest.Status == "Cancelled"
                    ? "This lab order was cancelled."
                    : "This lab order already has a result.");

        var now = clock.UtcNow;
        var result = new LabResult
        {
            LabRequestId = labRequest.Id,
            ResultValue = request.ResultValue.Trim(),
            Unit = string.IsNullOrWhiteSpace(request.Unit) ? null : request.Unit.Trim(),
            ReferenceRange = string.IsNullOrWhiteSpace(request.ReferenceRange) ? null : request.ReferenceRange.Trim(),
            ReportedAt = now
        };

        labRequest.Status = "Completed";
        await labResultRepository.AddAsync(result, cancellationToken).ConfigureAwait(false);
        await labRequestRepository.UpdateAsync(labRequest, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new AdminClinicVisitLabResultDto
        {
            Id = labRequest.Id,
            VisitId = visit.Id,
            VisitStart = visit.VisitStart,
            TestName = labRequest.TestName,
            Status = labRequest.Status,
            PickupCode = labRequest.PickupCode,
            RequestedAt = labRequest.RequestedAt,
            ResultValue = result.ResultValue,
            Unit = result.Unit,
            ReferenceRange = result.ReferenceRange,
            ReportedAt = result.ReportedAt
        };
    }
}
