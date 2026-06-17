using System.Linq;
using MediatR;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.PatientTelehealth.DTOs;
using RaphCare.Domain.Telemedicine;

namespace RaphCare.Application.Features.PatientTelehealth.Queries.GetMyTeleSessions;

public class GetMyTeleSessionsHandler : IRequestHandler<GetMyTeleSessionsQuery, PagedResult<PatientTeleSessionListItemDto>>
{
    private readonly IRepository<TeleSession> _repository;
    private readonly ICurrentUserService _currentUser;

    public GetMyTeleSessionsHandler(IRepository<TeleSession> repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<PatientTeleSessionListItemDto>> Handle(GetMyTeleSessionsQuery request, CancellationToken cancellationToken)
    {
        var patientId = _currentUser.CurrentPatientId
            ?? throw new ForbiddenAccessException("A patient profile is required to view telehealth sessions.");

        var paged = await _repository.SearchAsync(
            q => q.Where(s => s.PatientId == patientId).OrderByDescending(s => s.ScheduledStart),
            request.PageNumber,
            request.PageSize,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        var items = paged.Items.Select(s => new PatientTeleSessionListItemDto
        {
            Id = s.Id,
            ScheduledStart = s.ScheduledStart,
            Status = s.Status,
            Platform = s.Platform
        }).ToList();

        return new PagedResult<PatientTeleSessionListItemDto>
        {
            Items = items,
            TotalCount = paged.TotalCount,
            PageNumber = paged.PageNumber,
            PageSize = paged.PageSize
        };
    }
}
