using MediatR;
using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.Commands.CancelAdminClinicLabOrder;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Clinical;

namespace RaphCare.Application.Features.Organization.Commands.UndoAdminClinicLabOrder;

public sealed class UndoAdminClinicLabOrderHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IRepository<Visit> visitRepository,
    IRepository<LabRequest> labRequestRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UndoAdminClinicLabOrderCommand, AdminClinicVisitLabResultDto?>
{
    public async Task<AdminClinicVisitLabResultDto?> Handle(
        UndoAdminClinicLabOrderCommand request,
        CancellationToken cancellationToken)
    {
        await AdminClinicAuthorization.EnsureClinicStaffAsync(
                currentUserService,
                clinicStaffMembershipService,
                request.ClinicId,
                "Only hospital staff can undo a lab result.",
                cancellationToken)
            .ConfigureAwait(false);

        var labRequest = await labRequestRepository.GetByIdAsync(request.LabRequestId, cancellationToken).ConfigureAwait(false);
        if (labRequest is null)
            return null;

        var visit = await visitRepository.GetByIdAsync(labRequest.VisitId, cancellationToken).ConfigureAwait(false);
        if (visit is null || visit.ClinicId != request.ClinicId)
            return null;

        if (labRequest.Status is not ("Completed" or "Cancelled"))
            throw new BusinessRuleException("Only a completed or cancelled lab order can be put back to waiting.");

        labRequest.Status = "Pending";
        await labRequestRepository.UpdateAsync(labRequest, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var withResults = await labRequestRepository.SearchAsync(
            q => q.Where(l => l.Id == labRequest.Id).Include(l => l.LabResults),
            1,
            1,
            cancellationToken: cancellationToken).ConfigureAwait(false);
        var latest = LatestResult(withResults.Items);

        return CancelAdminClinicLabOrderHandler.Map(labRequest, visit, latest);
    }

    private static LabResult? LatestResult(IReadOnlyList<LabRequest> items)
    {
        if (items.Count == 0)
            return null;
        return items[0].LabResults.OrderByDescending(r => r.ReportedAt).FirstOrDefault();
    }
}
