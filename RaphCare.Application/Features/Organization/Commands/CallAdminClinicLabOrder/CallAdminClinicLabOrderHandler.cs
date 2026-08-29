using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.Commands.CancelAdminClinicLabOrder;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Clinical;

namespace RaphCare.Application.Features.Organization.Commands.CallAdminClinicLabOrder;

public sealed class CallAdminClinicLabOrderHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IRepository<Visit> visitRepository,
    IRepository<LabRequest> labRequestRepository,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CallAdminClinicLabOrderCommand, AdminClinicVisitLabResultDto?>
{
    public async Task<AdminClinicVisitLabResultDto?> Handle(
        CallAdminClinicLabOrderCommand request,
        CancellationToken cancellationToken)
    {
        await AdminClinicAuthorization.EnsureClinicStaffAsync(
                currentUserService,
                clinicStaffMembershipService,
                request.ClinicId,
                "Only hospital staff can call a pickup code.",
                cancellationToken)
            .ConfigureAwait(false);

        var labRequest = await labRequestRepository.GetByIdAsync(request.LabRequestId, cancellationToken)
            .ConfigureAwait(false);
        if (labRequest is null)
            return null;

        var visit = await visitRepository.GetByIdAsync(labRequest.VisitId, cancellationToken).ConfigureAwait(false);
        if (visit is null || visit.ClinicId != request.ClinicId)
            return null;

        if (labRequest.Status != "Pending")
            throw new BusinessRuleException("Only a waiting lab order can be called.");

        labRequest.CalledAt = clock.UtcNow;
        await labRequestRepository.UpdateAsync(labRequest, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return CancelAdminClinicLabOrderHandler.Map(labRequest, visit, result: null);
    }
}
