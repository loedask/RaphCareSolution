using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Clinical;

namespace RaphCare.Application.Features.Organization.Commands.CancelAdminClinicLabOrder;

public sealed class CancelAdminClinicLabOrderHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IRepository<Visit> visitRepository,
    IRepository<LabRequest> labRequestRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CancelAdminClinicLabOrderCommand, AdminClinicVisitLabResultDto?>
{
    public async Task<AdminClinicVisitLabResultDto?> Handle(
        CancelAdminClinicLabOrderCommand request,
        CancellationToken cancellationToken)
    {
        await AdminClinicAuthorization.EnsureClinicStaffAsync(
                currentUserService,
                clinicStaffMembershipService,
                request.ClinicId,
                "Only hospital staff can cancel a lab order.",
                cancellationToken)
            .ConfigureAwait(false);

        var labRequest = await labRequestRepository.GetByIdAsync(request.LabRequestId, cancellationToken).ConfigureAwait(false);
        if (labRequest is null)
            return null;

        var visit = await visitRepository.GetByIdAsync(labRequest.VisitId, cancellationToken).ConfigureAwait(false);
        if (visit is null || visit.ClinicId != request.ClinicId)
            return null;

        if (labRequest.Status != "Pending")
            throw new BusinessRuleException("Only a waiting lab order can be cancelled.");

        labRequest.Status = "Cancelled";
        await labRequestRepository.UpdateAsync(labRequest, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Map(labRequest, visit, result: null);
    }

    internal static AdminClinicVisitLabResultDto Map(LabRequest labRequest, Visit visit, LabResult? result) =>
        new()
        {
            Id = labRequest.Id,
            VisitId = visit.Id,
            VisitStart = visit.VisitStart,
            TestName = labRequest.TestName,
            Status = labRequest.Status,
            PickupCode = labRequest.PickupCode,
            RequestedAt = labRequest.RequestedAt,
            ResultValue = result?.ResultValue,
            Unit = result?.Unit,
            ReferenceRange = result?.ReferenceRange,
            ReportedAt = result?.ReportedAt
        };
}
