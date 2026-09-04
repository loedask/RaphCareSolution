using MediatR;
using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.MentalHealth.DTOs;
using RaphCare.Application.Features.Organization;
using RaphCare.Domain.MentalHealth;

namespace RaphCare.Application.Features.MentalHealth.Queries.GetTherapySessions;

public sealed class GetTherapySessionsHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IRepository<TherapySession> sessions)
    : IRequestHandler<GetTherapySessionsQuery, PagedResult<TherapySessionDto>>
{
    public async Task<PagedResult<TherapySessionDto>> Handle(
        GetTherapySessionsQuery request,
        CancellationToken cancellationToken)
    {
        await AdminClinicAuthorization.EnsureClinicStaffAsync(
                currentUserService,
                clinicStaffMembershipService,
                request.ClinicId,
                "Only hospital staff can view therapy sessions.",
                cancellationToken)
            .ConfigureAwait(false);

        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;
        var patientId = request.PatientId;

        var page = await sessions.SearchAsync(
            q =>
            {
                q = q.Where(s => s.ClinicId == request.ClinicId)
                    .Include(s => s.TherapyNotes)
                    .Include(s => s.CrisisFlags);
                if (patientId is Guid pid)
                    q = q.Where(s => s.PatientId == pid);
                return q.OrderByDescending(s => s.SessionStart);
            },
            pageNumber,
            pageSize,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        return new PagedResult<TherapySessionDto>
        {
            Items = page.Items.Select(TherapyMapper.ToSessionDto).ToList(),
            TotalCount = page.TotalCount,
            PageNumber = page.PageNumber,
            PageSize = page.PageSize
        };
    }
}
