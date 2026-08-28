using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Clinical;

namespace RaphCare.Application.Features.Organization.Commands.UpsertAdminClinicVisitSoapNote;

public sealed class UpsertAdminClinicVisitSoapNoteHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IUserRoleAssignmentService roleAssignmentService,
    IRepository<Visit> visitRepository,
    IRepository<SOAPNote> soapNoteRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpsertAdminClinicVisitSoapNoteCommand, AdminClinicVisitSoapNoteDto?>
{
    public async Task<AdminClinicVisitSoapNoteDto?> Handle(
        UpsertAdminClinicVisitSoapNoteCommand request,
        CancellationToken cancellationToken)
    {
        var visit = await AdminClinicVisitDocumentationHelper.GetWritableVisitAsync(
                currentUserService,
                clinicStaffMembershipService,
                roleAssignmentService,
                visitRepository,
                request.ClinicId,
                request.VisitId,
                "Only hospital administrators can save visit notes.",
                cancellationToken)
            .ConfigureAwait(false);
        if (visit is null)
            return null;

        var existingPage = await soapNoteRepository.SearchAsync(
            q => q.Where(s => s.VisitId == visit.Id),
            1,
            1,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        SOAPNote soap;
        if (existingPage.Items.Count == 0)
        {
            soap = new SOAPNote { VisitId = visit.Id };
            Apply(soap, request);
            await soapNoteRepository.AddAsync(soap, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            soap = await soapNoteRepository.GetByIdAsync(existingPage.Items[0].Id, cancellationToken).ConfigureAwait(false)
                ?? existingPage.Items[0];
            Apply(soap, request);
            await soapNoteRepository.UpdateAsync(soap, cancellationToken).ConfigureAwait(false);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new AdminClinicVisitSoapNoteDto
        {
            VisitId = visit.Id,
            VisitStart = visit.VisitStart,
            Subjective = soap.Subjective,
            Objective = soap.Objective,
            Assessment = soap.Assessment,
            Plan = soap.Plan
        };
    }

    private static void Apply(SOAPNote soap, UpsertAdminClinicVisitSoapNoteCommand request)
    {
        soap.Subjective = TrimToNull(request.Subjective);
        soap.Objective = TrimToNull(request.Objective);
        soap.Assessment = TrimToNull(request.Assessment);
        soap.Plan = TrimToNull(request.Plan);
    }

    private static string? TrimToNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
