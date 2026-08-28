using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Clinical;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicVisitNote;

public sealed class CreateAdminClinicVisitNoteHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IUserRoleAssignmentService roleAssignmentService,
    IRepository<Visit> visitRepository,
    IRepository<ClinicalNote> noteRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateAdminClinicVisitNoteCommand, AdminClinicVisitNoteDto?>
{
    public async Task<AdminClinicVisitNoteDto?> Handle(
        CreateAdminClinicVisitNoteCommand request,
        CancellationToken cancellationToken)
    {
        var visit = await AdminClinicVisitDocumentationHelper.GetWritableVisitAsync(
                currentUserService,
                clinicStaffMembershipService,
                roleAssignmentService,
                visitRepository,
                request.ClinicId,
                request.VisitId,
                "Only a doctor or hospital administrator can add visit notes.",
                cancellationToken)
            .ConfigureAwait(false);
        if (visit is null)
            return null;

        var note = new ClinicalNote
        {
            VisitId = visit.Id,
            Notes = request.Notes.Trim(),
            Category = string.IsNullOrWhiteSpace(request.Category) ? null : request.Category.Trim()
        };

        await noteRepository.AddAsync(note, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new AdminClinicVisitNoteDto
        {
            VisitId = visit.Id,
            VisitStart = visit.VisitStart,
            Notes = note.Notes,
            Category = note.Category
        };
    }
}
