using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Reporting.DTOs;
using RaphCare.Domain.Reporting;

namespace RaphCare.Application.Features.Reporting.Queries.GetDashboardSnapshot;

public sealed class GetDashboardSnapshotHandler(IRepository<DashboardSnapshot> repository)
    : IRequestHandler<GetDashboardSnapshotQuery, DashboardSnapshotDto>
{
    public async Task<DashboardSnapshotDto> Handle(GetDashboardSnapshotQuery request, CancellationToken cancellationToken)
    {
        var page = await repository.SearchAsync(
            q => q
                .Where(s => s.ClinicId == request.ClinicId && s.SnapshotDate <= request.SnapshotDate)
                .OrderByDescending(s => s.SnapshotDate),
            1,
            1,
            applyDefaultIdOrdering: false,
            cancellationToken).ConfigureAwait(false);

        var snapshot = page.Items.Count > 0 ? page.Items[0] : null;
        if (snapshot is null)
        {
            return new DashboardSnapshotDto
            {
                Id = Guid.Empty,
                ClinicId = request.ClinicId,
                SnapshotDate = request.SnapshotDate,
                SnapshotJson = "{}"
            };
        }

        return new DashboardSnapshotDto
        {
            Id = snapshot.Id,
            ClinicId = snapshot.ClinicId,
            SnapshotDate = snapshot.SnapshotDate,
            SnapshotJson = snapshot.SnapshotJson
        };
    }
}
