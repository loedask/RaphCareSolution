using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Clinical;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicVisitLabResult;

public sealed class CreateAdminClinicVisitLabResultHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IUserRoleAssignmentService roleAssignmentService,
    IRepository<Visit> visitRepository,
    IRepository<LabRequest> labRequestRepository,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateAdminClinicVisitLabResultCommand, AdminClinicVisitLabResultDto?>
{
    public async Task<AdminClinicVisitLabResultDto?> Handle(
        CreateAdminClinicVisitLabResultCommand request,
        CancellationToken cancellationToken)
    {
        var visit = await AdminClinicVisitDocumentationHelper.GetWritableVisitAsync(
                currentUserService,
                clinicStaffMembershipService,
                roleAssignmentService,
                visitRepository,
                request.ClinicId,
                request.VisitId,
                "Only hospital administrators can add lab results.",
                cancellationToken)
            .ConfigureAwait(false);
        if (visit is null)
            return null;

        var now = clock.UtcNow;
        var labRequest = new LabRequest
        {
            VisitId = visit.Id,
            TestName = request.TestName.Trim(),
            RequestedAt = now
        };
        labRequest.LabResults.Add(new LabResult
        {
            ResultValue = request.ResultValue.Trim(),
            Unit = string.IsNullOrWhiteSpace(request.Unit) ? null : request.Unit.Trim(),
            ReferenceRange = string.IsNullOrWhiteSpace(request.ReferenceRange) ? null : request.ReferenceRange.Trim(),
            ReportedAt = now
        });

        await labRequestRepository.AddAsync(labRequest, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var result = labRequest.LabResults.First();
        return new AdminClinicVisitLabResultDto
        {
            VisitId = visit.Id,
            VisitStart = visit.VisitStart,
            TestName = labRequest.TestName,
            ResultValue = result.ResultValue,
            Unit = result.Unit,
            ReferenceRange = result.ReferenceRange,
            ReportedAt = result.ReportedAt
        };
    }
}
