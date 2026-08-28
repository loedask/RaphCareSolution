using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Organization;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicVisitLabResult;

public sealed class CreateAdminClinicVisitLabResultHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IUserRoleAssignmentService roleAssignmentService,
    IRepository<Visit> visitRepository,
    IRepository<Prescription> prescriptionRepository,
    IRepository<LabRequest> labRequestRepository,
    IRepository<Clinic> clinicRepository,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork,
    IMediator mediator)
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
                "Only hospital administrators can order lab tests.",
                cancellationToken)
            .ConfigureAwait(false);
        if (visit is null)
            return null;

        var labRequest = new LabRequest
        {
            VisitId = visit.Id,
            TestName = request.TestName.Trim(),
            Priority = string.IsNullOrWhiteSpace(request.Priority) ? null : request.Priority.Trim(),
            RequestedAt = clock.UtcNow,
            Status = "Pending",
            PickupCode = await ClinicalPickupCode.AllocateAsync(
                prescriptionRepository,
                labRequestRepository,
                cancellationToken).ConfigureAwait(false)
        };

        await labRequestRepository.AddAsync(labRequest, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var clinic = await clinicRepository.GetByIdAsync(visit.ClinicId, cancellationToken).ConfigureAwait(false);
        await CollectionPatientNotifier.NotifyReadyAsync(
                mediator,
                visit.PatientId,
                clinic?.Name ?? string.Empty,
                labRequest.PickupCode,
                isLab: true,
                cancellationToken)
            .ConfigureAwait(false);

        return new AdminClinicVisitLabResultDto
        {
            Id = labRequest.Id,
            VisitId = visit.Id,
            VisitStart = visit.VisitStart,
            TestName = labRequest.TestName,
            Status = labRequest.Status,
            PickupCode = labRequest.PickupCode,
            RequestedAt = labRequest.RequestedAt
        };
    }
}
