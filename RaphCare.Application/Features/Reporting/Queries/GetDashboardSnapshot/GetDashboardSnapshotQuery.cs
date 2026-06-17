using MediatR;
using RaphCare.Application.Features.Reporting.DTOs;

namespace RaphCare.Application.Features.Reporting.Queries.GetDashboardSnapshot;

public class GetDashboardSnapshotQuery : IRequest<DashboardSnapshotDto>
{
    public Guid ClinicId { get; set; }
    public DateTime SnapshotDate { get; set; }
}

