using MediatR;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Reporting.DTOs;
using RaphCare.Domain.Reporting;

namespace RaphCare.Application.Features.Reporting.Queries.GetDashboardSnapshot;

public class GetDashboardSnapshotHandler : IRequestHandler<GetDashboardSnapshotQuery, DashboardSnapshotDto>
{
    private readonly IRepository<DashboardSnapshot> _repository;

    public GetDashboardSnapshotHandler(IRepository<DashboardSnapshot> repository)
    {
        _repository = repository;
    }

    public async Task<DashboardSnapshotDto> Handle(GetDashboardSnapshotQuery request, CancellationToken cancellationToken)
    {
        // Simplified: choose latest snapshot on/ before requested date for the clinic.
        var snapshots = await _repository.ListAsync(cancellationToken);

        var snapshot = snapshots
            .Where(s => s.ClinicId == request.ClinicId && s.SnapshotDate <= request.SnapshotDate)
            .OrderByDescending(s => s.SnapshotDate)
            .FirstOrDefault();

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

