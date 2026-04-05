using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Domain.MentalHealth;

namespace RaphCare.Application.Features.PatientMentalHealth.Commands.LogMyPatientMoodCheckIn;

public sealed class LogMyPatientMoodCheckInHandler(
    IRepository<MoodLog> logs,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser) : IRequestHandler<LogMyPatientMoodCheckInCommand, Guid>
{
    private readonly IRepository<MoodLog> _logs = logs;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICurrentUserService _currentUser = currentUser;

    public async Task<Guid> Handle(LogMyPatientMoodCheckInCommand request, CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required.");

        var entity = new MoodLog
        {
            PatientId = patientId,
            LoggedAt = DateTime.UtcNow,
            MoodScore = request.MoodScore,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            IsFlagged = request.MoodScore >= 3
        };

        await _logs.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return entity.Id;
    }
}
