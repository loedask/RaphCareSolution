using MediatR;
using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.MentalHealth.DTOs;
using RaphCare.Application.Features.Organization;
using RaphCare.Domain.MentalHealth;

namespace RaphCare.Application.Features.MentalHealth.Commands.AddTherapyNote;

public sealed class AddTherapyNoteHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IRepository<TherapySession> sessionRepository,
    IDateTimeProvider clock,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AddTherapyNoteCommand, TherapyNoteDto?>
{
    public async Task<TherapyNoteDto?> Handle(
        AddTherapyNoteCommand request,
        CancellationToken cancellationToken)
    {
        await AdminClinicAuthorization.EnsureClinicStaffAsync(
                currentUserService,
                clinicStaffMembershipService,
                request.ClinicId,
                "Only hospital staff can add a therapy note.",
                cancellationToken)
            .ConfigureAwait(false);

        var page = await sessionRepository.SearchAsync(
                q => q
                    .Where(s => s.Id == request.SessionId && s.ClinicId == request.ClinicId)
                    .Include(s => s.TherapyNotes)
                    .Include(s => s.CrisisFlags),
                1,
                1,
                applyDefaultIdOrdering: false,
                cancellationToken)
            .ConfigureAwait(false);

        var session = page.Items.Count > 0 ? page.Items[0] : null;
        if (session is null)
            return null;

        var note = new TherapyNote
        {
            TherapySessionId = session.Id,
            Notes = request.Notes.Trim(),
            Category = request.Category.Trim(),
            IsPrivate = request.IsPrivate,
            RecordedAt = clock.UtcNow
        };
        session.TherapyNotes.Add(note);

        if (!string.IsNullOrWhiteSpace(request.CrisisRiskLevel))
        {
            session.CrisisFlags.Add(new CrisisFlag
            {
                TherapySessionId = session.Id,
                PatientId = session.PatientId,
                RiskLevel = request.CrisisRiskLevel.Trim(),
                Description = string.IsNullOrWhiteSpace(request.CrisisDescription)
                    ? "Flagged during therapy note."
                    : request.CrisisDescription.Trim(),
                FlaggedAt = clock.UtcNow,
                IsResolved = false
            });
        }

        await sessionRepository.UpdateAsync(session, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new TherapyNoteDto
        {
            Id = note.Id,
            Notes = note.Notes,
            Category = note.Category,
            IsPrivate = note.IsPrivate,
            RecordedAt = note.RecordedAt
        };
    }
}
