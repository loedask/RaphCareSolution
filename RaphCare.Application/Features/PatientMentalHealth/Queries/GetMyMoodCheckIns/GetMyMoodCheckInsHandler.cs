using MediatR;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.PatientMentalHealth.DTOs;
using RaphCare.Domain.MentalHealth;

namespace RaphCare.Application.Features.PatientMentalHealth.Queries.GetMyMoodCheckIns;

public sealed class GetMyMoodCheckInsHandler : IRequestHandler<GetMyMoodCheckInsQuery, IReadOnlyList<PatientMoodCheckInDto>>
{
    private readonly IRepository<MoodLog> _logs;
    private readonly ICurrentUserService _currentUser;

    public GetMyMoodCheckInsHandler(IRepository<MoodLog> logs, ICurrentUserService currentUser)
    {
        _logs = logs;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<PatientMoodCheckInDto>> Handle(GetMyMoodCheckInsQuery request, CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required.");

        var pageSize = request.PageSize is < 1 or > 50 ? 14 : request.PageSize;
        var paged = await _logs.SearchAsync(
            q => q
                .Where(m => m.PatientId == patientId)
                .OrderByDescending(m => m.LoggedAt),
            1,
            pageSize,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        return paged.Items
            .Select(m => new PatientMoodCheckInDto
            {
                Id = m.Id,
                LoggedAt = m.LoggedAt,
                MoodScore = m.MoodScore
            })
            .ToList();
    }
}
